
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Serialization;
using Xunit;

namespace AsyncGuidance;

#region "async is viral"

public class AsyncIsViral
{
    //Once you go async, all of your callers SHOULD be async, since efforts to be async
    //amount to nothing unless the entire callstack is async. 

    //In many cases, being partially async can be worse than being entirely synchronous. 
    //Therefore it is best to go all in, and make everything async at once.

    //What this means is that you should make it async all the way to the main method or
    //the webapi controller.

    /// <summary>
    ///     BAD - This example uses the Task.Result and as a result blocks the current thread to wait
    ///     for the result. This is an example of sync over async.
    /// </summary>
    private int DoSomethingAsyncBad()
    {
        var result = CallDependencyAsync().Result;
        return result + 1;
    }

    /// <summary>
    ///     GOOD - This example uses the await keyword to get the result from CallDependencyAsync.
    /// </summary>
    private async Task<int> DoSomethingAsyncGood()
    {
        var result = await CallDependencyAsync();
        return result + 1;
    }

    private Task<int> CallDependencyAsync()
    {
        return Task.FromResult(1);
    }
}

#endregion

#region "async void"

public class AsyncVoid
{
    /*
Use of async void in ASP.NET Core applications is ALWAYS bad. Avoid it, never do it.
Typically, it's used when developers are trying to implement fire and forget patterns 
    triggered by a controller action.
Async void methods will crash the process if an exception is thrown.

What this means is that if the async void method crashes your asp.net application will crash 
    and it will not be able to serve requests.
You should use async task, that way any unhandled exception gets caught by the 
    TaskScheduler.UnobservedTaskException.
*/

    /// <summary>
    ///     BAD - Async void methods can't be tracked and therefore unhandled exceptions can
    ///     result in application crashes
    /// </summary>
    public async void BackgroundOperationBadAsync()
    {
        var result = await CallDependencyAsync();
        DoSomething(result);
    }

    /// <summary>
    ///     GOOD - Task-returning methods are better since unhandled exceptions
    ///     trigger the TaskScheduler.UnobservedTaskException.
    /// </summary>
    public async Task BackgroundOperationGoodAsync()
    {
        var result = await CallDependencyAsync();
        DoSomething(result);
    }

    private void DoSomething(int result)
    {
        throw new Exception("test");
    }

    private Task<int> CallDependencyAsync()
    {
        return Task.FromResult(1);
    }

    [Fact]
    public void async_void_crashes_the_process_in_case_of_an_exception()
    {
        Task.Run(BackgroundOperationBadAsync);
    }

    [Fact]
    public void async_task_triggers_an_unobservedTaskException()
    {
        Task.Run(BackgroundOperationGoodAsync);
    }
}

#endregion

#region "Prefer Task.FromResult over Task.Run for pre-computed or trivially computed data"

/*
 *
 *https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#prefer-taskfromresult-over-taskrun-for-pre-computed-or-trivially-computed-data
 * For pre-computed results, there's no need to call Task.Run, that will end up queuing a work item
 * to the thread pool that will immediately complete with the pre-computed value.
   Instead, use Task.FromResult, to create a task wrapping already computed data.
 */

public class PreferTaskFromResult
{
    /// <summary>
    /// BAD This example wastes a thread-pool thread to return a trivially computed value.
    /// </summary>
    public class MyLibraryBad
    {
        public Task<int> AddAsync(int a, int b)
        {
            return Task.Run(() => a + b);
        }
    }


    /// <summary>
    /// GOOD This example uses Task.FromResult to return the trivially computed value. 
    /// It does not use any extra threads as a result.
    /// </summary>
    public class MyLibraryGood
    {
        public Task<int> AddAsync(int a, int b)
        {
            return Task.FromResult(a + b);
        }
    }

    /*
 Using Task.FromResult will result in a Task allocation. Using ValueTask<T> can completely remove that allocation.
*/

    /// <summary>
    /// GOOD This example uses a ValueTask<int> to return the trivially computed value. 
    /// It does not use any extra threads as a result. 
    /// It also does not allocate an object on the managed heap.
    /// </summary>
    public class MyLibraryBetter
    {
        public ValueTask<int> AddAsync(int a, int b)
        {
            return new ValueTask<int>(a + b);
        }
    }
}

#endregion

#region "avoid taskRun for long running work that blocks the thread"

// https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#avoid-using-taskrun-for-long-running-work-that-blocks-the-thread
/*
Long running work in this context refers to a thread that's running for the lifetime of the application 
doing background work (like processing queue items, or sleeping and waking up to process some data).

Task.Run will queue a work item to the thread pool.

The assumption is that that work will finish quickly (or quickly enough to allow reusing that thread 
within some reasonable timeframe).

Stealing a thread-pool thread for long-running work is bad since it takes that thread away from 
other work that could be done(timer callbacks, task continuations etc).

Instead, spawn a new thread manually to do long running blocking work.

NOTE: 
The thread pool grows if you block threads but it's bad practice to do so.

NOTE:
Task.Factory.StartNew has an option TaskCreationOptions.LongRunning that under the covers 
creates a new thread and returns a Task that represents the execution. 
Using this properly requires several non-obvious parameters to be passed in to get the right behavior 
on all platforms.

NOTE: 
Don't use TaskCreationOptions.LongRunning with async code as this will create a new thread 
which will be destroyed after first await.

*/

public class LongRunningWork
{
    //public class QueueProcessorBad
    //{
    //    private readonly BlockingCollection<Message> _messageQueue = new BlockingCollection<Message>();

    //    public void StartProcessing()
    //    {

    //        //BAD This example steals a thread-pool thread forever, to execute queued work on a BlockingCollection<T>.
    //        Task.Run(ProcessQueue);
    //    }

    //    public void Enqueue(Message message)
    //    {
    //        _messageQueue.Add(message);
    //    }

    //    private void ProcessQueue()
    //    {
    //        foreach (var item in _messageQueue.GetConsumingEnumerable())
    //        {
    //            ProcessItem(item);
    //        }
    //    }

    //    private void ProcessItem(Message message) { }
    //}


    //public class QueueProcessorGood
    //{
    //    private readonly BlockingCollection<Message> _messageQueue = new BlockingCollection<Message>();

    //    public void StartProcessing()
    //    {
    //        //GOOD This example uses a dedicated thread to process the message queue instead of a thread-pool thread.
    //        var thread = new Thread(ProcessQueue)
    //        {
    //            // This is important as it allows the process to exit while this thread is running
    //            IsBackground = true
    //        };
    //        thread.Start();
    //    }

    //    public void Enqueue(Message message)
    //    {
    //        _messageQueue.Add(message);
    //    }

    //    private void ProcessQueue()
    //    {
    //        foreach (var item in _messageQueue.GetConsumingEnumerable())
    //        {
    //            ProcessItem(item);
    //        }
    //    }

    //    private void ProcessItem(Message message) { }
    //}
}

#endregion

#region "Avoid using taskResult and taskWait"

//https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#avoid-using-taskresult-and-taskwait
/*
There are very few ways to use Task.Result and Task.Wait correctly so the general advice is to completely
avoid using them in your code.

/*
Sync over async
Using Task.Result or Task.Wait to block wait on an asynchronous operation to complete is MUCH worse 
than calling a truly synchronous API to block.
This phenomenon is dubbed "Sync over async".

Here is what happens at a very high level:
- An asynchronous operation is kicked off.
- The calling thread is blocked waiting for that operation to complete.
- When the asynchronous operation completes, it unblocks the code waiting on that operation. 
  This takes place on another thread.

The result is that we need to use 2 threads instead of 1 to complete synchronous operations. 
This usually leads to thread-pool starvation and results in service outages.

*/
/*
Deadlocks
The SynchronizationContext is an abstraction that gives application models a chance to control 
where asynchronous continuations run.

ASP.NET (non-core), WPF and Windows Forms each have an implementation that will result in a deadlock 
if Task.Wait or Task.Result is used on the main thread.

This behavior has led to a bunch of "clever" code snippets that show the "right" way to block waiting 
for a Task.
The truth is, there's no good way to block waiting for a Task to complete.

NOTE: ASP.NET Core does not have a SynchronizationContext and is not prone to the deadlock problem.

*/

public class SyncOverAsync
{
    public string DoOperationBlocking()
    {
        // Bad - Blocking the thread that enters.
        // DoAsyncOperation will be scheduled on the default task scheduler, and remove the risk of deadlocking.
        // In the case of an exception, this method will throw an AggregateException wrapping the original exception.
        return Task.Run(() => DoAsyncOperation()).Result;
    }

    public string DoOperationBlocking2()
    {
        // Bad - Blocking the thread that enters.
        // DoAsyncOperation will be scheduled on the default task scheduler, and remove the risk of deadlocking.
        // In the case of an exception, this method will throw the exception without wrapping it in an AggregateException.
        return Task.Run(() => DoAsyncOperation()).GetAwaiter().GetResult();
    }

    public string DoOperationBlocking3()
    {
        // Bad - Blocking the thread that enters, and blocking the threadpool thread inside.
        // In the case of an exception, this method will throw an AggregateException containing another AggregateException, containing the original exception.
        return Task.Run(() => DoAsyncOperation().Result).Result;
    }

    public string DoOperationBlocking4()
    {
        // Bad - Blocking the thread that enters, and blocking the threadpool thread inside.
        return Task.Run(() => DoAsyncOperation().GetAwaiter().GetResult()).GetAwaiter().GetResult();
    }

    Task<string> DoAsyncOperation()
    {
        return Task.FromResult<string>("test");
    }

    public string DoOperationBlocking5()
    {
        // Bad - Blocking the thread that enters.
        // Bad - No effort has been made to prevent a present SynchonizationContext from becoming deadlocked.
        // In the case of an exception, this method will throw an AggregateException wrapping the original exception.
        return DoAsyncOperation().Result;
    }

    public string DoOperationBlocking6()
    {
        // Bad - Blocking the thread that enters.
        // Bad - No effort has been made to prevent a present SynchonizationContext from becoming deadlocked.
        return DoAsyncOperation().GetAwaiter().GetResult();
    }

    public string DoOperationBlocking7()
    {
        // Bad - Blocking the thread that enters.
        // Bad - No effort has been made to prevent a present SynchonizationContext from becoming deadlocked.
        var task = DoAsyncOperation();
        task.Wait();
        return task.GetAwaiter().GetResult();
    }
}

#endregion

#region "prefer await over continueWith"

class PreferAwait
{
    // https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#prefer-await-over-continuewith

    /*
    Task existed before the async/await keywords were introduced and as such provided ways to execute 
    continuations without relying on the language.
    
    Although these methods are still valid to use, we generally recommend that you prefer async/await 
    to using ContinueWith.
    
    ContinueWith also does not capture the SynchronizationContext and as a result is actually 
    semantically different to async/await.
    */
    /// <summary>
    /// BAD The example uses ContinueWith instead of async
    /// </summary>
    public Task<int> DoSomethingAsyncBad()
    {
        return CallDependencyAsync().ContinueWith(task => { return task.Result + 1; });
    }

    Task<int> CallDependencyAsync()
    {
        return Task.FromResult<int>(1);
    }

    /// <summary>
    /// GOOD This example uses the await keyword to get the result from CallDependencyAsync.
    /// </summary>
    public async Task<int> DoSomethingAsync()
    {
        var result = await CallDependencyAsync();
        return result + 1;
    }
}

#endregion

#region "always create tcs with runContinuationsAsync"

//https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#always-create-taskcompletionsourcet-with-taskcreationoptionsruncontinuationsasynchronously
/*
Always create TaskCompletionSource<T> with TaskCreationOptions.RunContinuationsAsynchronously
*/
/*
TaskCompletionSource<T> is an important building block for libraries trying to adapt things 
that are not inherently awaitable to be awaitable via a Task.

It is also commonly used to build higher-level operations (such as batching and other combinators) 
on top of existing asynchronous APIs.

By default, Task continuations will run inline on the same thread that calls 
Try/Set(Result/Exception/Canceled).

As a library author, this means having to understand that calling code can resume directly on your thread.
This is extremely dangerous and can result in deadlocks, thread-pool starvation, corruption of state 
(if code runs unexpectedly) and more.

Always use TaskCreationOptions.RunContinuationsAsynchronously when creating the TaskCompletionSource<T>.

This will dispatch the continuation onto the thread pool instead of executing it inline.
*/

public class CreateTcs
{
    /// <summary>
    ///  BAD This example does not use TaskCreationOptions.RunContinuationsAsynchronously when creating the TaskCompletionSource<T>.
    /// </summary>
    public Task<int> DoSomethingBadAsync()
    {
        var tcs = new TaskCompletionSource<int>();

        //var operation = new LegacyAsyncOperation();
        //operation.Completed += result =>
        //{
        //    // Code awaiting on this task will resume on this thread!
        //    tcs.SetResult(result);
        //};

        return tcs.Task;
    }

    /// <summary>
    /// GOOD This example uses TaskCreationOptions.RunContinuationsAsynchronously when creating the TaskCompletionSource<T>.
    /// </summary>
    public Task<int> DoSomethingAsync()
    {
        var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        //var operation = new LegacyAsyncOperation();
        //operation.Completed += result =>
        //{
        //    // Code awaiting on this task will resume on a different thread-pool thread
        //    tcs.SetResult(result);
        //};

        return tcs.Task;
    }

    /*
    NOTE: There are 2 enums that look alike. 
    TaskCreationOptions.RunContinuationsAsynchronously and TaskContinuationOptions.RunContinuationsAsynchronously. 
    Be careful not to confuse their usage.
    */
}

#endregion

#region "always dispose cancellationTokenSource used for timeouts"

public class DisposeCts
{
    IHttpClientFactory _httpClientFactory;
    //https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#always-dispose-cancellationtokensources-used-for-timeouts

    /*
    CancellationTokenSource objects that are used for timeouts (are created with timers or uses the CancelAfter method), 
    can put pressure on the timer queue if not disposed.
    */

    public DisposeCts(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// BAD This example does not dispose the CancellationTokenSource and as a result the timer stays in the queue for 10 seconds 
    /// after each request is made.
    /// </summary>
    public async Task<Stream> HttpClientAsyncWithCancellationBad()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        using (var client = _httpClientFactory.CreateClient())
        {
            var response = await client.GetAsync("http://backend/api/1", cts.Token);
            return await response.Content.ReadAsStreamAsync();
        }
    }

    /// <summary>
    /// GOOD This example disposes the CancellationTokenSource and properly removes the timer from the queue.
    /// </summary>
    public async Task<Stream> HttpClientAsyncWithCancellationGood()
    {
        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
        {
            using (var client = _httpClientFactory.CreateClient())
            {
                var response = await client.GetAsync("http://backend/api/1", cts.Token);
                return await response.Content.ReadAsStreamAsync();
            }
        }
    }
}

#endregion

#region "always flow CancellationToken(s) to APIs that take a cancellation token"

public class FlowCancellationTokens
{
    private readonly Stream _stream;

    public FlowCancellationTokens(Stream stream)
    {
        _stream = stream;
    }

    //https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#always-flow-cancellationtokens-to-apis-that-take-a-cancellationtoken

    /*
    Cancellation is cooperative in .NET.
    Everything in the call-chain has to be explicitly passed the CancellationToken in order for it to work well.
    This means you need to explicitly pass the token into other APIs that take a token if you want cancellation to be 
    most effective.
    */


    /// <summary>
    /// BAD This example neglects to pass the CancellationToken to Stream.ReadAsync making the operation effectively not cancellable.
    /// </summary>
    public async Task<string> DoAsyncThingBad(CancellationToken cancellationToken = default)
    {
        byte[] buffer = new byte[1024];
        // We forgot to pass flow cancellationToken to ReadAsync
        int read = await _stream.ReadAsync(buffer, 0, buffer.Length);
        return Encoding.UTF8.GetString(buffer, 0, read);
    }

    /// <summary>
    /// GOOD This example passes the CancellationToken into Stream.ReadAsync.
    /// </summary>
    public async Task<string> DoAsyncThing(CancellationToken cancellationToken = default)
    {
        byte[] buffer = new byte[1024];
        // This properly flows cancellationToken to ReadAsync
        int read = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
        return Encoding.UTF8.GetString(buffer, 0, read);
    }
}

#endregion

#region "Always call FlushAsync on StreamWriter(s) or Stream(s) before calling Dispose"

class AlwaysFlushAsync
{
    private readonly IApplicationBuilder _app;

    public AlwaysFlushAsync(IApplicationBuilder app)
    {
        _app = app;
    }

    //https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#always-call-flushasync-on-streamwriters-or-streams-before-calling-dispose

    /*
    Always call FlushAsync on StreamWriter(s) or Stream(s) before calling Dispose

    When writing to a Stream or StreamWriter, even if the asynchronous overloads are used for writing, the underlying data might be buffered.
    When data is buffered, disposing the Stream or StreamWriter via the Dispose method will synchronously write/flush, which results in blocking the thread and could lead to thread-pool starvation.
    Either use the asynchronous DisposeAsync method (for example via await using) or call FlushAsync before calling Dispose.
    */

    /*
    NOTE: This is only problematic if the underlying subsystem does IO.
    */

    public void Test1()
    {
        //BAD This example ends up blocking the request by writing synchronously to the HTTP-response body.
        _app.Run(async context =>
        {
            // The implicit Dispose call will synchronously write to the response body
            using (var streamWriter = new StreamWriter(context.Response.Body))
            {
                await streamWriter.WriteAsync("Hello World");
            }
        });

        // GOOD This example asynchronously flushes any buffered data before disposing the StreamWriter
        _app.Run(async context =>
        {
            using (var streamWriter = new StreamWriter(context.Response.Body))
            {
                await streamWriter.WriteAsync("Hello World");

                // Force an asynchronous flush
                await streamWriter.FlushAsync();
            }
        });


        // GOOD This example asynchronously flushes any buffered data while disposing the StreamWriter.
        _app.Run(async context =>
        {
            // The implicit AsyncDispose call will flush asynchronously
            await using (var streamWriter = new StreamWriter(context.Response.Body))
            {
                await streamWriter.WriteAsync("Hello World");
            }
        });
    }
}

#endregion

#region "prefer async_await over directly returning Task"

public class PreferAsyncAwait
{
    //https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#prefer-asyncawait-over-directly-returning-task

    /*

    There are benefits to using the async/await keyword instead of directly returning the Task:

     - Asynchronous and synchronous exceptions are normalized to always be asynchronous.
     - The code is easier to modify (consider adding a using, for example).
     - Diagnostics of asynchronous methods are easier (debugging hangs etc).
     - Exceptions thrown will be automatically wrapped in the returned Task instead of surprising the caller with an actual exception.
     - Async locals will not leak out of async methods. If you set an async local in a non-async method, it will "leak" out of that call.

    */

    /// <summary>
    /// BAD This example directly returns the Task to the caller.
    /// </summary>
    public Task<int> DoSomethingAsyncBad()
    {
        return CallDependencyAsync();
    }

    Task<int> CallDependencyAsync()
    {
        throw new NotImplementedException();
    }


    /// <summary>
    ///  GOOD This examples uses async/await instead of directly returning the Task.
    /// </summary>
    public async Task<int> DoSomethingAsyncGood()
    {
        return await CallDependencyAsync();
    }

    /*
    NOTE: There are performance considerations when using an async state machine over directly returning the Task. 
    It's always faster to directly return the Task since it does less work but you end up changing the behavior 
    and potentially losing some of the benefits of the async state machine.
    */
}

#endregion

#region "Scenario1 Timer Callbacks"

public class ScenarioTimerCallbacks
{
    //https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#timer-callbacks

    /// <summary>
    /// BAD The Timer callback is void-returning and we have asynchronous work to execute. 
    /// This example uses async void to accomplish it and as a result can crash the process if an exception occurs.
    /// </summary>
    public class PingerBad1
    {
        private readonly Timer _timer;
        private readonly HttpClient _client;

        public PingerBad1(HttpClient client)
        {
            _client = client;
            _timer = new Timer(Heartbeat, null, 1000, 1000);
        }

        public async void Heartbeat(object state)
        {
            await _client.GetAsync("http://mybackend/api/ping");
        }
    }

    /// <summary>
    /// BAD This attempts to block in the Timer callback. 
    /// This may result in thread-pool starvation and is an example of sync over async.
    /// </summary>
    public class PingerBad2
    {
        private readonly Timer _timer;
        private readonly HttpClient _client;

        public PingerBad2(HttpClient client)
        {
            _client = client;
            _timer = new Timer(Heartbeat, null, 1000, 1000);
        }

        public void Heartbeat(object state)
        {
            _client.GetAsync("http://mybackend/api/ping").GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// GOOD This example uses an async Task-based method and discards the Task in the Timer callback. 
    /// If this method fails, it will not crash the process. 
    /// Instead, it will fire the TaskScheduler.UnobservedTaskException event.
    /// </summary>
    public class PingerGood
    {
        private readonly Timer _timer;
        private readonly HttpClient _client;

        public PingerGood(HttpClient client)
        {
            _client = client;
            _timer = new Timer(Heartbeat, null, 1000, 1000);
        }

        public void Heartbeat(object state)
        {
            // Discard the result
            _ = DoAsyncPing();
        }

        private async Task DoAsyncPing()
        {
            await _client.GetAsync("http://mybackend/api/ping");
        }
    }
}

#endregion

#region "Scenario2 implicit async void delegates"

public class ImplicitAsyncVoidDelegates
{
    /*
        Imagine a BackgroundQueue with a FireAndForget that takes a callback. 
        This method will execute the callback at some time in the future.
    */

    /// <summary>
    /// BAD - This will force callers to either block in the callback or use an async void delegate.
    /// </summary>
    public class BackgroundQueueBad
    {
        public static void FireAndForget(Action action)
        {
        }
    }

    void Main()
    {
        var httpClient = new HttpClient();
        //BAD - This calling code is creating an async void method implicitly. 
        //The compiler fully supports this today.
        BackgroundQueueBad.FireAndForget(async () => { await httpClient.GetAsync("http://pinger/api/1"); });

        Console.ReadLine();
    }

    /// <summary>
    /// GOOD This BackgroundQueue implementation offers both sync and async callback overloads.
    /// </summary>
    public class BackgroundQueue
    {
        public static void FireAndForget(Action action)
        {
        }

        public static void FireAndForgetAsync(Func<Task> action)
        {
        }
    }
}

#endregion

#region "Scenario3 ConcurrentDictionary GetOrAdd"

public class ConcurrentDictionaryGetOrAdd
{
    //https://github.com/davidfowl/AspNetCoreDiagnosticScenarios/blob/master/AsyncGuidance.md#concurrentdictionarygetoradd

    /*
    It's pretty common to cache the result of an asynchronous operation and ConcurrentDictionary is a good data structure for doing that.
    GetOrAdd is a convenience API for trying to get an item if it's already there or adding it if it isn't.
    The callback is synchronous so it's tempting to write code that uses Task.Result to produce the value of an asynchronous process 
    but that can lead to thread-pool starvation.
    */

    /// <summary>
    ///  BAD - This may result in thread-pool starvation since we're blocking the request thread if the person data is not cached.
    /// </summary>
    public class PersonControllerBad1
    {
        // This cache needs expiration
        private static ConcurrentDictionary<int, Person> _cache
            = new ConcurrentDictionary<int, Person>();

        public Person Get(int id)
        {
            var person =
                _cache.GetOrAdd(id, (key) => Task.FromResult(new Person()).Result); //_db.People.FindAsync(key).Result);
            return person;
        }
    }


    /// <summary>
    /// GOOD This implementation won't result in thread-pool starvation since we're storing a task instead of the result itself.
    /// ConcurrentDictionary.GetOrAdd, when accessed concurrently, may run the value-constructing delegate multiple times. 
    /// This can result in needlessly kicking off the same potentially expensive computation multiple times.
    /// </summary>
    public class PersonControllerGood1
    {
        // This cache needs expiration
        private static ConcurrentDictionary<int, Task<Person>> _cache
            = new ConcurrentDictionary<int, Task<Person>>();

        public async Task<Person> Get(int id)
        {
            var person =
                await _cache.GetOrAdd(id, (key) => Task.FromResult(new Person())); //_db.People.FindAsync(key));
            return person;
        }
    }


    /// <summary>
    /// GOOD This implementation prevents the delegate from being executed multiple times, 
    /// by using the async lazy pattern: even if construction of the AsyncLazy instance happens multiple times ("cheap" operation), 
    /// the delegate will be called only once.
    /// </summary>
    public class PersonControllerGood2
    {
        // This cache needs expiration
        private static ConcurrentDictionary<int, AsyncLazy<Person>> _cache
            = new ConcurrentDictionary<int, AsyncLazy<Person>>();


        public async Task<Person> Get(int id)
        {
            var person = await _cache.GetOrAdd(id, (key) => new AsyncLazy<Person>(() => Task.FromResult(new Person())))
                .Value; //_db.People.FindAsync(key)))

            return person;
        }

        private class AsyncLazy<T> : Lazy<Task<T>>
        {
            public AsyncLazy(Func<Task<T>> valueFactory) : base(valueFactory)
            {
            }
        }
    }
}

public class Person
{
}

#endregion

#region "Scenario4 Constructors"

public class Constructors
{
    /*
Constructors are synchronous.
If you need to initialize some logic that may be asynchronous, there are a couple of patterns for dealing with this.
*/

    /*
    Here's an example of using a client API that needs to connect asynchronously before use.
    */

    public interface IRemoteConnectionFactory
    {
        Task<IRemoteConnection> ConnectAsync();
    }

    public interface IRemoteConnection
    {
        Task PublishAsync(string channel, string message);
        Task DisposeAsync();
    }

    /// <summary>
    /// BAD This example uses Task.Result to get the connection in the constructor. 
    /// This could lead to thread-pool starvation and deadlocks.
    /// </summary>
    public class ServiceBad
    {
        private readonly IRemoteConnection _connection;

        public ServiceBad(IRemoteConnectionFactory connectionFactory)
        {
            _connection = connectionFactory.ConnectAsync().Result;
        }
    }

    /// <summary>
    /// GOOD This implementation uses a static factory pattern in order to allow asynchronous construction:
    /// </summary>
    public class ServiceGood
    {
        private readonly IRemoteConnection _connection;

        private ServiceGood(IRemoteConnection connection)
        {
            _connection = connection;
        }

        public static async Task<ServiceGood> CreateAsync(IRemoteConnectionFactory connectionFactory)
        {
            return new ServiceGood(await connectionFactory.ConnectAsync());
        }
    }
}

#endregion

