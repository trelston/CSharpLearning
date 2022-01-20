using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using AutoMapper.Configuration.Annotations;
using AutoMapper.Extensions.EnumMapping;

namespace AutomapperSamples;

#region "Configuration Examples"

#region "profile instances"

public class ProfileInstances
{
    /*
        https://docs.automapper.org/en/latest/Configuration.html#profile-instances
        Profile Instances
    */

    /*
        A good way to organize your mapping configurations is with profiles.
        Create classes that inherit from Profile and put the configuration in the constructor:
    */
    public class OrganizationProfile : Profile
    {
        public OrganizationProfile()
        {
            CreateMap<Foo, FooDto>();
            // Use CreateMap... Etc.. here (Profile methods are the same as configuration methods)
        }
    }

    public class Foo
    {
    }

    public class FooDto
    {
    }

    /*
    Configuration inside a profile only applies to maps inside the profile. 
    Configuration applied to the root configuration applies to all maps created.
    Just use Configuration profiles. 
    I suspect that if you add maps to the root configuration in the aspnet core Startup class 
    then those mappings will override the configuration profile maps.
    I also suspect that the above suspicion may be incorrect but i am too lazy to check.
    Hence, the phrase, just use configuration profiles for mapping.
    */
}

#endregion

#region "assembly scanning for auto configuration"

public class AssemblyScanning
{
    /*
        https://docs.automapper.org/en/latest/Configuration.html#assembly-scanning-for-auto-configuration

        Profiles can be added to the main mapper configuration in a number of ways, either directly:
    */
    public void Test()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<OrganizationProfile>();
            cfg.AddProfile(new OrganizationProfile());
        });

        /*
            or by automatically scanning for profiles:
            AutoMapper will scan the designated assemblies for classes inheriting from Profile and add them to the configuration.
        */

        // Scan for all profiles in an assembly
        // ... using instance approach:
        var myAssembly = Assembly.GetExecutingAssembly();
        var config = new MapperConfiguration(cfg => { cfg.AddMaps(myAssembly); });
        var configuration1 = new MapperConfiguration(cfg => cfg.AddMaps(myAssembly));

        // Can also use assembly names:
        var configuration2 = new MapperConfiguration(cfg =>
            cfg.AddMaps("Foo.UI", "Foo.Core")
        );

        // Or marker types for assemblies:
        var configuration3 = new MapperConfiguration(cfg =>
            cfg.AddMaps(typeof(Foo), typeof(FooDto))
        );
    }

    public class OrganizationProfile : Profile
    {
        public OrganizationProfile()
        {
            CreateMap<Foo, FooDto>();
            // Use CreateMap... Etc.. here (Profile methods are the same as configuration methods)
        }
    }

    public class Foo
    {
    }

    public class FooDto
    {
    }
}

#endregion

#region "Naming Convention"

public class NamingConvention
{
    /*
        https://docs.automapper.org/en/latest/Configuration.html#naming-conventions
        Naming Conventions
    */

    /*
        You can set the source and destination naming conventions.
        The below code will map the following properties to each other: property_name -> PropertyName
    */
    public void Test()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.SourceMemberNamingConvention = new LowerUnderscoreNamingConvention();
            cfg.DestinationMemberNamingConvention = new PascalCaseNamingConvention();
        });
    }

    /*
        You can also set this at a per profile level
    */

    public class OrganizationProfile : Profile
    {
        public OrganizationProfile()
        {
            SourceMemberNamingConvention = new LowerUnderscoreNamingConvention();
            DestinationMemberNamingConvention = new PascalCaseNamingConvention();
            //Put your CreateMap... Etc.. here
        }
    }

    //If you don’t need a naming convention, you can use the ExactMatchNamingConvention.

    //This is extremely useful i think, especially when using a database or a third party api with a weird naming convention
    //This way you can create models that serve only this api or database breaking the c# naming convention but then mapping them
    //to those beautiful c# models with proper naming convention.
}

#endregion

#region "replacing individual characters"

public class ReplacingCharacters
{
    /*
        https://docs.automapper.org/en/latest/Configuration.html#replacing-characters
        Replacing Characters
    */

    /*
        We want to replace the individual characters, and perhaps translate a word:
    */

    /*
        You can also replace individual characters or entire words in source members during member name matching:
    */

    public void Test()
    {
        var configuration = new MapperConfiguration(c =>
        {
            c.ReplaceMemberName("Ä", "A");
            c.ReplaceMemberName("í", "i");
            c.ReplaceMemberName("Airlina", "Airline");
        });
    }

    public class Source
    {
        public int Value { get; set; }
        public int Ävíator { get; set; }
        public int SubAirlinaFlight { get; set; }
    }

    public class Destination
    {
        public int Value { get; set; }
        public int Aviator { get; set; }
        public int SubAirlineFlight { get; set; }
    }
    //Again very useful feature given my comment on naming convention
}

#endregion

#region "Recognizing pre/postfixes"

/*
    https://docs.automapper.org/en/latest/Configuration.html#recognizing-pre-postfixes
    Recognizing pre/postfixes
*/
/*
    Sometimes your source/destination properties will have common pre/postfixes 
    that cause you to have to do a bunch of custom member mappings because the names don’t match up. 
    To address this, you can recognize pre/postfixes:
*/

internal class PrePostFixes
{
    private void Main()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.RecognizePrefixes("frm");
            cfg.CreateMap<Source, Dest>();
        });

        configuration.AssertConfigurationIsValid();

        /*
            By default AutoMapper recognizes the prefix “Get”, if you need to clear the prefix:
        */
        var configuration1 = new MapperConfiguration(cfg =>
        {
            cfg.ClearPrefixes();
            cfg.RecognizePrefixes("tmp");
        });
    }

    public class Source
    {
        public int frmValue { get; set; }
        public int frmValue2 { get; set; }
    }

    public class Dest
    {
        public int Value { get; set; }
        public int Value2 { get; set; }
    }
}

//This is i think even better than naming conventions because a lot of real world databases have some
//real crazy prefixes and postfixes. This feature along with naming conventions and replacing characters feature
//should cover the crazy naming conventions you find yourself working with out there in the real world.

#endregion

#region "Global property/field filtering"

/*
    https://docs.automapper.org/en/latest/Configuration.html#global-property-field-filtering
    Global property/field filtering
*/
/*
    By default, AutoMapper tries to map every public property/field. 
    You can filter out properties/fields with the property/field filters:
*/

internal class GlobalPropertyFiltering
{
    public void Test()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            // don't map any fields
            cfg.ShouldMapField = fi => false;

            // map properties with a public or private getter
            cfg.ShouldMapProperty = pi =>
                pi.GetMethod != null && (pi.GetMethod.IsPublic || pi.GetMethod.IsPrivate);
        });
    }
}

//Very useful if you need to exclude a few properties.
//This really maps to a real world scenario where you always have exceptions to the rule.
//Use this feature sparingly and only use it if you really cannot avoid it.
//Keep things simple for the next developer who will work on your code.

#endregion

#region "Configuring visibility"

/*
    https://docs.automapper.org/en/latest/Configuration.html#configuring-visibility
    Configuring visibility
*/
/*
    By default, AutoMapper only recognizes public members. 
    It can map to private setters, but will skip internal/private methods and properties if the entire property is 
    private/internal. 
    To instruct AutoMapper to recognize members with other visibilities, override the default filters ShouldMapField 
    and/or ShouldMapProperty :
*/

internal class ConfiguringVisibility
{
    public void Test()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            // map properties with public or internal getters
            /*
                Map configurations will now recognize internal/private members.
            */
            cfg.ShouldMapProperty = p => p.GetMethod.IsPublic || p.GetMethod.IsAssembly;
            cfg.CreateMap<Source, Destination>();
        });
    }

    private class Source
    {
    }

    private class Destination
    {
    }
}

//Useful feature but again use it sparingly and only if you do not have control over the source model.
//Otherwise the simplest thing to do is that if you want it to be mapped, make it public.

#endregion

#region "Configuration Compilation"

/*
    https://docs.automapper.org/en/latest/Configuration.html#configuration-compilation
    Configuration compilation
*/
/*
    Because expression compilation can be a bit resource intensive, 
    AutoMapper lazily compiles the type map plans on first map.
    
    However, this behavior is not always desirable, so you can tell AutoMapper to compile its mappings directly:
*/

internal class ConfigurationCompilation
{
    public void Test()
    {
        var configuration = new MapperConfiguration(cfg => { });
        configuration.CompileMappings();

        /*
        For a few hundred mappings, this may take a couple of seconds. 
        If it’s a lot more than that, you probably have some really big execution plans.
        */

        /*
        https://docs.automapper.org/en/latest/Configuration.html#long-compilation-times
        Long compilation times
        */
    }
}

#endregion

#region "Configuration Validation"

/*
    https://docs.automapper.org/en/latest/Configuration-validation.html#configuration-validation
    Configuration Validation
*/
/*
Hand-rolled mapping code, though tedious, has the advantage of being testable. 
One of the inspirations behind AutoMapper was to eliminate not just the custom mapping code, but eliminate the need 
for manual testing. 
Because the mapping from source to destination is convention-based, you will still need to test your configuration.
*/
/*
AutoMapper provides configuration testing in the form of the AssertConfigurationIsValid method. 
Suppose we have slightly misconfigured our source and destination types:
*/

internal class ConfigurationValidation
{
    private void Main()
    {
        /*
        In the Destination type, we probably fat-fingered the destination property. 
        Other typical issues are source member renames. 
        To test our configuration, we simply create a unit test that sets up the configuration and executes 
        the AssertConfigurationIsValid method:
        */

        var configuration = new MapperConfiguration(cfg =>
            cfg.CreateMap<Source, Destination>());

        configuration.AssertConfigurationIsValid();

        /*
        Executing this code produces an AutoMapperConfigurationException, with a descriptive message. 
        AutoMapper checks to make sure that every single Destination type member has a corresponding type member on the 
        source type.
        */

        /*
        This is interesting but do use it in a unit test, not in a real application.
        */
    }

    public class Source
    {
        public int SomeValue { get; set; }
    }

    public class Destination
    {
        public int SomeValuefff { get; set; }
    }
}

#endregion

#region "Selecting members to validate"

/*
    https://docs.automapper.org/en/latest/Configuration-validation.html#selecting-members-to-validate
    Selecting members to validate
*/

/*
    By default, AutoMapper uses the destination type to validate members. 
    It assumes that all destination members need to be mapped. 
    To modify this behavior, use the CreateMap overload to specify which member list to validate against:
*/
internal class MemberValidation
{
    public void Test()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>(MemberList.Source);

            /*To skip validation altogether for this map, use MemberList.None.*/
            cfg.CreateMap<Source2, Destination2>(MemberList.None);
        });
    }

    private class Source
    {
    }

    private class Destination
    {
    }

    private class Source2
    {
    }

    private class Destination2
    {
    }
}

#endregion

#region "Overriding configuration errors"

/*
    https://docs.automapper.org/en/latest/Configuration-validation.html#overriding-configuration-errors
    Overriding configuration errors
*/
/*
To fix a configuration error (besides renaming the source/destination members), you have three choices for providing an 
alternate configuration:

    - Custom Value Resolvers
    - Projection
    - Use the Ignore() option

With the third option, we have a member on the destination type that we will fill with alternative means, 
and not through the Map operation.
*/

internal class OverridingConfigurationErrors
{
    public void Test()
    {
        var configuration = new MapperConfiguration(cfg =>
            cfg.CreateMap<Source, Destination>()
                .ForMember(dest => dest.SomeValuefff,
                    opt => opt.Ignore())
        );
    }

    private class Source
    {
        public int SomeValue { get; set; }
    }

    private class Destination
    {
        public int SomeValuefff { get; set; }
    }

    /*
    Just one those features you must avoid if you have control over the source and destination code.
    */
}

#endregion

#endregion

#region "Projection"

/*
    https://docs.automapper.org/en/latest/Projection.html#projection
    Projection
*/
/*
    Projection transforms a source to a destination beyond flattening the object model.
    Without extra configuration, AutoMapper requires a flattened destination to match the source type’s naming structure.
    When you want to project source values into a destination that does not exactly match the source structure, 
    you must specify custom member mapping definitions.
*/

class Projection
{
    //For example, we might want to turn this source structure:
    public class CalendarEvent
    {
        public DateTime Date { get; set; }
        public string Title { get; set; }
    }

    //Into something that works better for an input form on a web page:
    public class CalendarEventForm
    {
        public DateTime EventDate { get; set; }
        public int EventHour { get; set; }
        public int EventMinute { get; set; }
        public string Title { get; set; }
    }

    public void Test()
    {
        /*
            Because the names of the destination properties do not exactly match the source property 
            (CalendarEvent.Date would need to be CalendarEventForm.EventDate), 
            we need to specify custom member mappings in our type map configuration:
        */

        // Model
        var calendarEvent = new CalendarEvent
        {
            Date = new DateTime(2008, 12, 15, 20, 30, 0),
            Title = "Company Holiday Party"
        };

        // Configure AutoMapper
        var configuration = new MapperConfiguration(cfg =>
            cfg.CreateMap<CalendarEvent, CalendarEventForm>()
                .ForMember(dest => dest.EventDate, opt => opt.MapFrom(src => src.Date.Date))
                .ForMember(dest => dest.EventHour, opt => opt.MapFrom(src => src.Date.Hour))
                .ForMember(dest => dest.EventMinute, opt => opt.MapFrom(src => src.Date.Minute)));

        // Perform mapping
        IMapper mapper = new Mapper(configuration);
        CalendarEventForm form = mapper.Map<CalendarEvent, CalendarEventForm>(calendarEvent);
    }
    /*
	This is a perfectly valid real-world scenario.
	In a lot of these cases you do not have access to the source model or you would like to use a common source model
	but different destinations with different models like UI, web, model.
	I hate it when they use words like Projection though. What the hell does it even mean. What are we projecting?
	We are just mapping.
	*/
}

#endregion

#region "Nested Mapping"

/*
    https://docs.automapper.org/en/latest/Nested-mappings.html#nested-mappings
    Nested Mappings
*/
/*
As the mapping engine executes the mapping, it can use one of a variety of methods to resolve a destination member value.

One of these methods is to use another type map, where the source member type and destination member type 
are also configured in the mapping configuration.

This allows us to not only flatten our source types, but create complex destination types as well.

For example, our source type might contain another complex type:
*/

class NestedMappings
{
    public class OuterSource
    {
        public int Value { get; set; }
        public InnerSource Inner { get; set; }
    }

    public class InnerSource
    {
        public int OtherValue { get; set; }
    }

    /*
    We could simply flatten the OuterSource.Inner.OtherValue to one InnerOtherValue property, 
    but we might also want to create a corresponding complex type for the Inner property:
    */

    public class OuterDest
    {
        public int Value { get; set; }
        public InnerDest Inner { get; set; }
    }

    public class InnerDest
    {
        public int OtherValue { get; set; }
    }

    /*
    In that case, we would need to configure the additional source/destination type mappings:
    */

    void Test()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<OuterSource, OuterDest>();
            cfg.CreateMap<InnerSource, InnerDest>();
        });
        config.AssertConfigurationIsValid();

        var source = new OuterSource
        {
            Value = 5,
            Inner = new InnerSource {OtherValue = 15}
        };
        var mapper = config.CreateMapper();
        var dest = mapper.Map<OuterSource, OuterDest>(source);
    }

    /*
    A few things to note here:

        Order of configuring types does not matter
        Call to Map does not need to specify any inner type mappings, only the type map to use for the source value passed in

    With both flattening and nested mappings, we can create a variety of destination shapes to suit whatever our needs may be.
    */
}

#endregion

#region "List and Arrays"

#region "List_arrays"

/*
    https://docs.automapper.org/en/latest/Lists-and-arrays.html#lists-and-arrays
    Lists and Arrays
*/
/*
AutoMapper only requires configuration of element types, not of any array or list type that might be used.
For example, we might have a simple source and destination type:
*/

class Lists_Arrays
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class Destination
    {
        public int Value { get; set; }
    }

    void Test()
    {
        /*
        All the basic generic collection types are supported:
        */

        var configuration = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>());

        var sources = new[]
        {
            new Source {Value = 5},
            new Source {Value = 6},
            new Source {Value = 7}
        };

        var mapper = configuration.CreateMapper();
        var ienumerableDest = mapper.Map<Source[], IEnumerable<Destination>>(sources);
        var icollectionDest = mapper.Map<Source[], ICollection<Destination>>(sources);
        var ilistDest = mapper.Map<Source[], IList<Destination>>(sources);
        var listDest = mapper.Map<Source[], List<Destination>>(sources);
        var arrayDest = mapper.Map<Source[], Destination[]>(sources);

        /*
        When mapping to an existing collection, the destination collection is cleared first. 
        If this is not what you want, take a look at AutoMapper.Collection.
        */
    }
}

#endregion

#region "Handling Null Collections"

/*
    https://docs.automapper.org/en/latest/Lists-and-arrays.html#handling-null-collections
    Handling null collections
*/
/*
    When mapping a collection property, if the source value is null AutoMapper will map the destination field 
    to an empty collection rather than setting the destination value to null.

    This aligns with the behavior of Entity Framework and Framework Design Guidelines 
    that believe C# references, arrays, lists, collections, dictionaries and IEnumerables should NEVER be null, ever.

    This behavior can be changed by setting the AllowNullCollections property to true when configuring the mapper.
*/

class HandlingNullCollections
{
    class Source
    {
    }

    class Destination
    {
    }

    public void Test()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullCollections = true;
            cfg.CreateMap<Source, Destination>();
        });
    }

    /*
        The setting can be applied globally and can be overridden per profile and per member with AllowNull and DoNotAllowNull.
    */
}

#endregion

#region "Polymorphic element types in collections"

/*
    https://docs.automapper.org/en/latest/Lists-and-arrays.html#polymorphic-element-types-in-collections
    Polymorphic element types in collections
*/
/*
    Many times, we might have a hierarchy of types in both our source and destination types.
    AutoMapper supports polymorphic arrays and collections, such that derived source/destination types are used if found.
*/

class PolymorphicCollections
{
    public class ParentSource
    {
        public int Value1 { get; set; }
    }

    public class ChildSource : ParentSource
    {
        public int Value2 { get; set; }
    }

    public class ParentDestination
    {
        public int Value1 { get; set; }
    }

    public class ChildDestination : ParentDestination
    {
        public int Value2 { get; set; }
    }

    /*
    AutoMapper still requires explicit configuration for child mappings, 
    as AutoMapper cannot “guess” which specific child destination mapping to use. 

    Here is an example of the above types:
    */

    void Test()
    {
        var configuration = new MapperConfiguration(c =>
        {
            c.CreateMap<ParentSource, ParentDestination>()
                .Include<ChildSource, ChildDestination>();
            c.CreateMap<ChildSource, ChildDestination>();
        });

        var sources = new[]
        {
            new ParentSource(),
            new ChildSource(),
            new ParentSource()
        };

        var mapper = configuration.CreateMapper();
        var destinations = mapper.Map<ParentSource[], ParentDestination[]>(sources);
    }
}

#endregion

#endregion

#region "Construction"

/*
    https://docs.automapper.org/en/latest/Construction.html#construction
    Construction
*/
/*
	AutoMapper can map to destination constructors based on source members:
*/

class Construction_Test
{
    public class Source
    {
        public int Value { get; set; }
    }

    public class SourceDto
    {
        public SourceDto(int value)
        {
            _value = value;
        }

        private int _value;

        public int Value
        {
            get { return _value; }
        }
    }

    /*
        If the destination constructor parameter names don’t match, you can modify them at config time:
    */

    public class SourceDto2
    {
        public SourceDto2(int valueParamSomeOtherName)
        {
            _value = valueParamSomeOtherName;
        }

        private int _value;

        public int Value
        {
            get { return _value; }
        }
    }


    void Test()
    {
        var configuration = new MapperConfiguration(cfg => cfg.CreateMap<Source, SourceDto>());

        //This works for both LINQ projections and in-memory mapping.
        var configuration1 = new MapperConfiguration(cfg =>
            cfg.CreateMap<Source, SourceDto2>()
                .ForCtorParam("valueParamSomeOtherName", opt => opt.MapFrom(src => src.Value)));


        //You can also disable constructor mapping:
        var configuration2 = new MapperConfiguration(cfg => cfg.DisableConstructorMapping());

        //You can configure which constructors are considered for the destination object:
        // don't map private constructors
        var configuration3 = new MapperConfiguration(cfg => cfg.ShouldUseConstructor = ci => !ci.IsPrivate);
    }
}

#endregion

#region "Flatenning"

#region "Flatenning"

/*
https://docs.automapper.org/en/latest/Flattening.html#flattening
Flattening
*/
/*
One of the common usages of object-object mapping is to take a complex object model and flatten it to a simpler model.
You can take a complex model such as:
*/

class Flatenning
{
    public class Order
    {
        private readonly IList<OrderLineItem> _orderLineItems = new List<OrderLineItem>();

        public Customer Customer { get; set; }

        public OrderLineItem[] GetOrderLineItems()
        {
            return _orderLineItems.ToArray();
        }

        public void AddOrderLineItem(Product product, int quantity)
        {
            _orderLineItems.Add(new OrderLineItem(product, quantity));
        }

        public decimal GetTotal()
        {
            return _orderLineItems.Sum(li => li.GetTotal());
        }
    }

    public class Product
    {
        public decimal Price { get; set; }
        public string Name { get; set; }
    }

    public class OrderLineItem
    {
        public OrderLineItem(Product product, int quantity)
        {
            Product = product;
            Quantity = quantity;
        }

        public Product Product { get; private set; }
        public int Quantity { get; private set; }

        public decimal GetTotal()
        {
            return Quantity * Product.Price;
        }
    }

    public class Customer
    {
        public string Name { get; set; }
    }

    /*
    We want to flatten this complex Order object into a simpler OrderDto that contains only the data needed for a certain scenario:
    */

    public class OrderDto
    {
        public string CustomerName { get; set; }
        public decimal Total { get; set; }
    }

    /*
    When you configure a source/destination type pair in AutoMapper, 
    the configurator attempts to match properties and methods on the source type to properties on the destination type.

    If for any property on the destination type a property, method, or a method prefixed with “Get” does not exist on the source type, 
    AutoMapper splits the destination member name into individual words (by PascalCase conventions).
    */


    void Test()
    {
        // Complex model

        var customer = new Customer
        {
            Name = "George Costanza"
        };
        var order = new Order
        {
            Customer = customer
        };
        var bosco = new Product
        {
            Name = "Bosco",
            Price = 4.99m
        };
        order.AddOrderLineItem(bosco, 15);

        // Configure AutoMapper
        var configuration = new MapperConfiguration(cfg => cfg.CreateMap<Order, OrderDto>());

        // Perform mapping
        var mapper = configuration.CreateMapper();
        OrderDto dto = mapper.Map<Order, OrderDto>(order);


        /*
        On the OrderDto type, the Total property matched to the GetTotal() method on Order.

        The CustomerName property matched to the Customer.Name property on Order.

        As long as we name our destination properties appropriately, we do not need to configure individual property matching.

        If you want to disable this behavior, you can use the ExactMatchNamingConvention:
        */

        var cfg = new MapperConfiguration(cfg =>
        {
            cfg.DestinationMemberNamingConvention = new ExactMatchNamingConvention();
            cfg.CreateMap<Order, OrderDto>();
        });

        // Perform mapping
        var mapper1 = cfg.CreateMapper();
        OrderDto dto1 = mapper1.Map<Order, OrderDto>(order);
    }
}

#endregion

#region "Include Members"

class IncludeMembers
{
    /*
        https://docs.automapper.org/en/latest/Flattening.html#includemembers
        IncludeMembers
    */

    /*
        If you need more control when flattening, you can use IncludeMembers.

        You can map members of a child object to the destination object when you already have a map from the child type 
        to the destination type (unlike the classic flattening that doesn’t require a map for the child type).
    */

    class Source
    {
        public string Name { get; set; }
        public InnerSource InnerSource { get; set; }
        public OtherInnerSource OtherInnerSource { get; set; }
    }

    class InnerSource
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    class OtherInnerSource
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }

    class Destination
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
    }

    void Main()
    {
        var cfg1 = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>().IncludeMembers(s => s.InnerSource, s => s.OtherInnerSource);
            cfg.CreateMap<InnerSource, Destination>(MemberList.None);
            cfg.CreateMap<OtherInnerSource, Destination>();
        });

        var source = new Source
        {
            Name = "name",
            InnerSource = new InnerSource {Description = "description"},
            OtherInnerSource = new OtherInnerSource {Title = "title"}
        };

        var mapper = cfg1.CreateMapper();
        var destination = mapper.Map<Destination>(source);


        /*
        The order of the parameters in the IncludeMembers call is relevant.

        When mapping a destination member, the first match wins, starting with the source object itself 
        and then with the included child objects in the order you specified. 

        So in the example above, Name is mapped from the source object itself and Description from InnerSource because it’s the first match.

        Note that this matching is static, it happens at configuration time, not at Map time, 
        and the runtime types of the child objects are not considered.
        */
    }
}

#endregion

#endregion

#region "Reverse Mapping and Unflatenning"

#region "Reverse Mapping and Unflatenning"

/*
https://docs.automapper.org/en/latest/Reverse-Mapping-and-Unflattening.html#reverse-mapping-and-unflattening
Reverse Mapping and Unflattening
*/
/*
Starting with 6.1.0, AutoMapper now supports richer reverse mapping support. Given our entities:
*/

class ReverseMappingUnflattening
{
    public class Order
    {
        public decimal Total { get; set; }
        public Customer Customer { get; set; }
    }

    public class Customer
    {
        public string Name { get; set; }
    }


    /*
    We can flatten this into a DTO:
    */

    public class OrderDto
    {
        public decimal Total { get; set; }
        public string CustomerName { get; set; }
    }


    void Test()
    {
        /*
        We can map both directions, including unflattening:
        */

        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>()
                .ReverseMap();
        });

        /*
            By calling ReverseMap, AutoMapper creates a reverse mapping configuration that includes unflattening:
        */
        var customer = new Customer
        {
            Name = "Bob"
        };

        var order = new Order
        {
            Customer = customer,
            Total = 15.8m
        };

        var mapper = configuration.CreateMapper();
        var orderDto = mapper.Map<Order, OrderDto>(order);

        orderDto.CustomerName = "Joe";

        mapper.Map(orderDto, order);

        /*
            Unflattening is only configured for ReverseMap. If you want unflattening, 
            you must configure Entity -> Dto then call ReverseMap to create an unflattening type map configuration from the Dto -> Entity.
        */
    }
}

#endregion

#region "Customizing reverse mapping"

/*
https://docs.automapper.org/en/latest/Reverse-Mapping-and-Unflattening.html#customizing-reverse-mapping
Customizing reverse mapping
*/

class CustomizingReverseMapping
{
    public class Order
    {
        public decimal Total { get; set; }
        public Customer Customer { get; set; }
    }

    public class Customer
    {
        public string Name { get; set; }
    }

    public class OrderDto
    {
        public decimal Total { get; set; }
        public string CustomerName { get; set; }
    }


    void Main()
    {
        /*
        AutoMapper will automatically reverse map “Customer.Name” from “CustomerName” based on the original flattening. 
        If you use MapFrom, AutoMapper will attempt to reverse the map:
        */
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>()
                //I think this is useful when you do not want the default unflatenning to take effect
                .ForMember(d => d.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
                .ReverseMap();
        });

        /*
        As long as the MapFrom path are member accessors, AutoMapper will unflatten from the same path (CustomerName => Customer.Name)
        */

        /*
        If you need to customize this, for a reverse map you can use ForPath:
        */
        var configuration1 = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>()
                .ForMember(d => d.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
                .ReverseMap()
                .ForPath(s => s.Customer.Name, opt => opt.MapFrom(src => src.CustomerName));
            /*
                 For most cases you shouldn’t need this, as the original MapFrom will be reversed for you. 
                Use ForPath when the path to get and set the values are different.
                This is when you just want to go wild.
             */
        });

        /*
        If you do not want unflattening behavior, you can remove the call to ReverseMap and create two separate maps. Or, you can use Ignore:
        */
        var configuration2 = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>()
                .ForMember(d => d.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
                .ReverseMap()
                //This is when you want to expressly prohibit the automatic reverse mapping for that specific property.
                //Specifically, you do this when you want to behave like a douchebag and make code for everyone who has to
                //read this code after you, want to know where you live.
                .ForPath(s => s.Customer.Name, opt => opt.Ignore());
        });
    }
}

#endregion

#endregion

#region "Mapping Inheritance"

#region "Inheritance Mapping Priorities"

/*
https://docs.automapper.org/en/latest/Mapping-inheritance.html#inheritance-mapping-priorities
Inheritance Mapping Priorities
*/
/*
This introduces additional complexity because there are multiple ways a property can be mapped. 
The priority of these sources are as follows

    Explicit Mapping (using .MapFrom())
    Inherited Explicit Mapping
    Ignore Property Mapping
    Convention Mapping (Properties that are matched via convention)

To demonstrate this, lets modify our classes shown above
*/

class InheritanceMappingProperties
{
    //Domain Objects
    public class Order
    {
    }

    public class OnlineOrder : Order
    {
        public string Referrer { get; set; }
    }

    public class MailOrder : Order
    {
    }

    //Dtos
    public class OrderDto
    {
        public string Referrer { get; set; }
    }

    void Test()
    {
        //Mappings
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>()
                .Include<OnlineOrder, OrderDto>()
                .Include<MailOrder, OrderDto>()
                .ForMember(o => o.Referrer, m => m.Ignore());
            cfg.CreateMap<OnlineOrder, OrderDto>();
            cfg.CreateMap<MailOrder, OrderDto>();
        });

        // Perform Mapping
        var order = new OnlineOrder {Referrer = "google"};
        var mapper = configuration.CreateMapper();
        var mapped = mapper.Map(order, order.GetType(), typeof(OrderDto));

        /*
        Notice that in our mapping configuration, 
        we have ignored Referrer (because it doesn’t exist in the order base class) 
        and that has a higher priority than convention mapping, so the property doesn’t get mapped.
        */
    }
}

#endregion

#region "Runtime polymorphism"

/*
    https://docs.automapper.org/en/latest/Mapping-inheritance.html#runtime-polymorphism
    Runtime polymorphism
*/

class RuntimePolymorphism
{
    public class Order
    {
    }

    public class OnlineOrder : Order
    {
    }

    public class MailOrder : Order
    {
    }

    public class OrderDto
    {
    }

    public class OnlineOrderDto : OrderDto
    {
    }

    public class MailOrderDto : OrderDto
    {
    }

    void Test()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>()
                .Include<OnlineOrder, OnlineOrderDto>()
                .Include<MailOrder, MailOrderDto>();
            cfg.CreateMap<OnlineOrder, OnlineOrderDto>();
            cfg.CreateMap<MailOrder, MailOrderDto>();
        });

        // Perform Mapping
        var order = new OnlineOrder();
        var mapper = configuration.CreateMapper();
        var mapped = mapper.Map(order, order.GetType(), typeof(OrderDto));

        /*
        You will notice that because the mapped object is a OnlineOrder, 
        AutoMapper has seen you have a more specific mapping for OnlineOrder than OrderDto, and automatically chosen that.
        */
    }
}

#endregion

#region "Specifying inheritance in derived classes"

/*
https://docs.automapper.org/en/latest/Mapping-inheritance.html#specifying-inheritance-in-derived-classes
Specifying inheritance in derived classes
*/
/*
Instead of configuring inheritance from the base class, you can specify inheritance from the derived classes:
*/

class DerivedClassesInheritance
{
    public class Order
    {
        public string OrderId { get; set; }
    }

    public class OnlineOrder : Order
    {
    }

    public class MailOrder : Order
    {
    }

    public class OrderDto
    {
        public string Id { get; set; }
    }

    public class OnlineOrderDto : OrderDto
    {
    }

    public class MailOrderDto : OrderDto
    {
    }

    public void Test()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>()
                .ForMember(o => o.Id, m => m.MapFrom(s => s.OrderId));
            cfg.CreateMap<OnlineOrder, OnlineOrderDto>()
                .IncludeBase<Order, OrderDto>();
            cfg.CreateMap<MailOrder, MailOrderDto>()
                .IncludeBase<Order, OrderDto>();
        });
    }
}

#endregion

#endregion

#region "Attribute Mapping"

#region "Type Map configuration"

/*
    https://docs.automapper.org/en/latest/Attribute-mapping.html#type-map-configuration
    Type Map configuration
*/
/*
    In addition to fluent configuration is the ability to declare and configure maps via attributes. 
    Attribute maps can supplement or replace fluent mapping configuration.
*/

class TypeMapConfiguration
{
    class Order
    {
    }

    /*
        To declare an attribute map, decorate your destination type with the AutoMapAttribute:
    */
    [AutoMap(typeof(Order))]
    public class OrderDto
    {
        // destination members
    }
    /*
        This is equivalent to a CreateMap<Order, OrderDto>() configuration.
    */


    /*
        In order to search for maps to configure, use the AddMaps method:
    */
    void Main()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddMaps("MyAssembly"));

        var mapper = new Mapper(configuration);
    }
    /*
        AddMaps looks for fluent map configuration (Profile classes) and attribute-based mappings.
    */
}

#endregion

#region "Customizing type map configuration"

/*
    https://docs.automapper.org/en/latest/Attribute-mapping.html#customizing-type-map-configuration
    Customizing type map configuration
*/
/*
To customize the overall type map configuration, you can set the following properties on the AutoMapAttribute:

	ReverseMap (bool)
    ConstructUsingServiceLocator (bool)
    MaxDepth (int)
    PreserveReferences (bool)
    DisableCtorValidation (bool)
    IncludeAllDerived (bool)
    TypeConverter (Type)

These all correspond to the similar fluent mapping configuration options. Only the sourceType value is required to map.

*/

#endregion

#region "Member Configuration"

/*
https://docs.automapper.org/en/latest/Attribute-mapping.html#member-configuration
Member configuration
*/

class MemberConfiguration
{
    /*
        For attribute-based maps, you can decorate individual members with additional configuration. 
        Because attributes have limitations in C# (no expressions, for example), the configuration options available are a bit limited.

        Member-based attributes are declared in the AutoMapper.Configuration.Annotations namespace.

        If the attribute-based configuration is not available or will not work, 
        you can combine both attribute and profile-based maps (though this may be confusing).
    */

    /*
        https://docs.automapper.org/en/latest/Attribute-mapping.html#ignoring-members
        Ignoring members
    */

    /*
        Use the IgnoreAttribute to ignore an individual destination member from mapping and/or validation:
    */

    /*
        Use the IgnoreAttribute to ignore an individual destination member from mapping and/or validation:
    */

    [AutoMap(typeof(Order))]
    public class OrderDto
    {
        [Ignore] public decimal Total { get; set; }
    }

    class Order
    {
        public int OrderTotal { get; set; }
    }

    /*
    https://docs.automapper.org/en/latest/Attribute-mapping.html#redirecting-to-a-different-source-member
    Redirecting to a different source member
    */

    /*
    It is not possible to use MapFrom with an expression in an attribute, 
    but SourceMemberAttribute can redirect to a separate named member:
    */

    [AutoMap(typeof(Order))]
    public class OrderDto1
    {
        [SourceMember("OrderTotal")] public decimal Total { get; set; }
    }

    /*
    Or use the nameof operator:
    */

    [AutoMap(typeof(Order))]
    public class OrderDto2
    {
        [SourceMember(nameof(Order.OrderTotal))]
        public decimal Total { get; set; }
    }

    /*
    You cannot flatten with this attribute, only redirect source type members (i.e. no “Order.Customer.Office.Name” in the name). 
    Configuring flattening is only available with the fluent configuration.

    Additional attribute-based configuration options include:

        MapAtRuntimeAttribute
        MappingOrderAttribute
        NullSubstituteAttribute
        UseExistingValueAttribute
        ValueConverterAttribute
        ValueResolverAttribute

    Each corresponds to the same fluent configuration mapping option.
    */
}

#endregion

#endregion

#region "Dynamic and Expando Mapping"

/*
    https://docs.automapper.org/en/latest/Dynamic-and-ExpandoObject-Mapping.html#dynamic-and-expandoobject-mapping
    Dynamic and ExpandoObject Mapping
*/
/*
AutoMapper can map to/from dynamic objects without any explicit configuration:
*/

class DynamicMapping
{
    public class Foo
    {
        public int Bar { get; set; }
        public int Baz { get; set; }
        public Foo InnerFoo { get; set; }
    }

    void Test()
    {
        //dynamic foo = new MyDynamicObject();
        //foo.Bar = 5;
        //foo.Baz = 6;

        //var configuration = new MapperConfiguration(cfg => { });
        //var mapper = configuration.CreateMapper();

        //var result = mapper.Map<Foo>(foo);
        //result.Bar.ShouldEqual(5);
        //result.Baz.ShouldEqual(6);

        //dynamic foo2 = mapper.Map<MyDynamicObject>(result);
        //foo2.Bar.ShouldEqual(5);
        //foo2.Baz.ShouldEqual(6);


        ///*
        //Similarly you can map straight from Dictionary<string, object> to objects, 
        //AutoMapper will line up the keys with property names. 
        //For mapping to destination child objects, you can use the dot notation.
        //*/

        //var result1 = mapper.Map<Foo>(new Dictionary<string, object> { ["InnerFoo.Bar"] = 42 });
        //result1.InnerFoo.Bar.ShouldEqual(42);
    }
}

#endregion

#region "Open Generics"

class OpenGenerics
{
    /*
        https://docs.automapper.org/en/latest/Open-Generics.html#open-generics
        Open Generics
    */

    /*
    AutoMapper can support an open generic type map. Create a map for the open generic types:
    */

    public class Source<T>
    {
        public T Value { get; set; }
    }

    public class Destination<T>
    {
        public T Value { get; set; }
    }

    void Test()
    {
        // Create the mapping
        var configuration = new MapperConfiguration(cfg => cfg.CreateMap(typeof(Source<>), typeof(Destination<>)));
        var mapper = configuration.CreateMapper();

        /*
        You don’t need to create maps for closed generic types. 
        AutoMapper will apply any configuration from the open generic mapping to the closed mapping at runtime:
        */
        var source = new Source<int> {Value = 10};

        var dest = mapper.Map<Source<int>, Destination<int>>(source);


        /*
        Because C# only allows closed generic type parameters, 
        you have to use the System.Type version of CreateMap to create your open generic type maps.

        From there, you can use all of the mapping configuration available 
        and the open generic configuration will be applied to the closed type map at runtime.

        AutoMapper will skip open generic type maps during configuration validation, since you can still create closed types that don’t convert, 
        such as Source<Foo> -> Destination<Bar> where there is no conversion from Foo to Bar.
        */

        /*
        You can also create an open generic type converter:
        */
        var configuration1 = new MapperConfiguration(cfg =>
            cfg.CreateMap(typeof(Source<>), typeof(Destination<>)).ConvertUsing(typeof(Converter<,>)));
        /*
        The closed type from Source will be the first generic argument, and the closed type of Destination 
        will be the second argument to close Converter<,>.
        */
    }
}

#endregion

#region "Queryable Extensions"

/*
https://docs.automapper.org/en/latest/Queryable-Extensions.html#queryable-extensions
Queryable Extensions
*/
/*
When using an ORM such as NHibernate or Entity Framework with AutoMapper’s standard mapper.Map functions,
you may notice that the ORM will query all the fields of all the objects within a graph 
when AutoMapper is attempting to map the results to a destination type.

If your ORM exposes IQueryables, you can use AutoMapper’s QueryableExtensions helper methods to address this key pain.

Using Entity Framework for an example, say that you have an entity OrderLine with a relationship with an entity Item.
If you want to map this to an OrderLineDTO with the Item’s Name property, the standard mapper.Map call will result in Entity Framework 
querying the entire OrderLine and Item table.

Use this approach instead.

Given the following entities:
*/

class QueryableExtensions
{
    public class OrderLine
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Item Item { get; set; }
        public decimal Quantity { get; set; }
    }

    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    /*And the following DTO:*/

    public class OrderLineDTO
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string Item { get; set; }
        public decimal Quantity { get; set; }
    }

    /*You can use the Queryable Extensions like so:*/

    void Test()
    {
        var configuration = new MapperConfiguration(cfg =>
            cfg.CreateProjection<OrderLine, OrderLineDTO>()
                .ForMember(dto => dto.Item, conf => conf.MapFrom(ol => ol.Item.Name)));
    }
}

#endregion

#region "Enum Mapping"

class EnumMapping
{
    /*
https://docs.automapper.org/en/latest/Enum-Mapping.html#automapper-extensions-enummapping
AutoMapper.Extensions.EnumMapping
*/

    /*
    The built-in enum mapper is not configurable, it can only be replaced.
    Alternatively, AutoMapper supports convention based mapping of enum values in a separate package AutoMapper.Extensions.EnumMapping.

    For method CreateMap this library provide a ConvertUsingEnumMapping method. 
    This method add all default mappings from source to destination enum values.

    If you want to change some mappings, then you can use MapValue method. This is a chainable method.

    Default the enum values are mapped by value (explicitly: MapByValue()), but it is possible to map by name calling MapByName().
    */


    public enum Source
    {
        Default = 0,
        First = 1,
        Second = 2
    }

    public enum Destination
    {
        Default = 0,
        Second = 2
    }

    internal class YourProfile : Profile
    {
        public YourProfile()
        {
            CreateMap<Source, Destination>()
                .ConvertUsingEnumMapping(opt => opt
                    // optional: .MapByValue() or MapByName(), without configuration MapByValue is used
                    .MapValue(Source.First, Destination.Default)
                )
                .ReverseMap(); // to support Destination to Source mapping, including custom mappings of ConvertUsingEnumMapping
        }
    }

    /*
    https://docs.automapper.org/en/latest/Enum-Mapping.html#default-convention
    Default Convention

    The package AutoMapper.Extensions.EnumMapping will map all values from Source type to Destination type 
    if both enum types have the same value (or by name or by value).

    All Source enum values which have no Target equivalent, will throw an exception if EnumMappingValidation is enabled.
    */

    /*
    https://docs.automapper.org/en/latest/Enum-Mapping.html#reversemap-convention
    ReverseMap Convention

    For method ReverseMap the same convention is used as for default mappings, but it also respects override enum value mappings if possible.

    The following steps determines the reversed overrides:



    */

    void Test()
    {
    }
}

#endregion

#region "Custom Type Converters"

class CustomTypeConverters
{
    /*
https://docs.automapper.org/en/latest/Custom-type-converters.html#custom-type-converters
Custom Type Converters

Sometimes, you need to take complete control over the conversion of one type to another.

This is typically when one type looks nothing like the other, a conversion function already exists, 
and you would like to go from a “looser” type to a stronger type, such as a source type of string to a destination type of Int32.

For example, suppose we have a source type of:
*/

    public class Source
    {
        public string Value1 { get; set; }
        public string Value2 { get; set; }
        public string Value3 { get; set; }
    }

    /*
    But you would like to map it to:
    */

    public class Destination
    {
        public int Value1 { get; set; }
        public DateTime Value2 { get; set; }
        public Type Value3 { get; set; }
    }

    /*
    If we were to try and map these two types as-is, AutoMapper would throw an exception (at map time and configuration-checking time), 
    as AutoMapper does not know about any mapping from string to int, DateTime or Type.

    To create maps for these types, we must supply a custom type converter, and we have three ways of doing so:
    void ConvertUsing(Func<TSource, TDestination> mappingFunction);
    void ConvertUsing(ITypeConverter<TSource, TDestination> converter);
    void ConvertUsing<TTypeConverter>() where TTypeConverter : ITypeConverter<TSource, TDestination>;

    The first option is simply any function that takes a source and returns a destination (there are several overloads too).

    This works for simple cases, but becomes unwieldy for larger ones. 
    In more difficult cases, we can create a custom ITypeConverter<TSource, TDestination>:
    public interface ITypeConverter<in TSource, TDestination>
    {
        TDestination Convert(TSource source, TDestination destination, ResolutionContext context);
    }

    And supply AutoMapper with either an instance of a custom type converter, or simply the type, which AutoMapper will instantiate at run time. 
    The mapping configuration for our above source/destination types then becomes:

    */

    public class DateTimeTypeConverter : ITypeConverter<string, DateTime>
    {
        public DateTime Convert(string source, DateTime destination, ResolutionContext context)
        {
            return System.Convert.ToDateTime(source);
        }
    }

    public class TypeTypeConverter : ITypeConverter<string, Type>
    {
        public Type Convert(string source, Type destination, ResolutionContext context)
        {
            return Assembly.GetExecutingAssembly().GetType(source);
        }
    }

    void Test()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<string, int>().ConvertUsing(s => Convert.ToInt32(s));
            cfg.CreateMap<string, DateTime>().ConvertUsing(new DateTimeTypeConverter());
            cfg.CreateMap<string, Type>().ConvertUsing<TypeTypeConverter>();
            cfg.CreateMap<Source, Destination>();
        });
        configuration.AssertConfigurationIsValid();
        var mapper = configuration.CreateMapper();

        var source = new Source
        {
            Value1 = "5",
            Value2 = "01/01/2000",
            Value3 = "AutoMapperSamples.GlobalTypeConverters.GlobalTypeConverters+Destination"
        };

        Destination result = mapper.Map<Source, Destination>(source);
    }
}

#endregion

#region "Custom Value Resolvers"

#region "Custom Value Resolvers"

class CustomValueResolvers
{
    /*
https://docs.automapper.org/en/latest/Custom-value-resolvers.html#custom-value-resolvers
Custom Value Resolvers
*/

    /*
     We might want to have a calculated value just during mapping:
    */

    public class Source
    {
        public int Value1 { get; set; }
        public int Value2 { get; set; }
    }

    public class Destination
    {
        public int Total { get; set; }
    }

    /*
    For whatever reason, we want Total to be the sum of the source Value properties.
    For some other reason, we can’t or shouldn’t put this logic on our Source type.
    To supply a custom value resolver, we’ll need to first create a type that implements IValueResolver:

    public interface IValueResolver<in TSource, in TDestination, TDestMember>
    {
        TDestMember Resolve(TSource source, TDestination destination, TDestMember destMember, ResolutionContext context);
    }

    The ResolutionContext contains all of the contextual information for the current resolution operation, 
    such as source type, destination type, source value and so on. An example implementation:
    */
    public class CustomResolver : IValueResolver<Source, Destination, int>
    {
        public int Resolve(Source source, Destination destination, int member, ResolutionContext context)
        {
            return source.Value1 + source.Value2;
        }
    }

    /*
    Once we have our IValueResolver implementation, 
    we’ll need to tell AutoMapper to use this custom value resolver when resolving a specific destination member.

    We have several options in telling AutoMapper a custom value resolver to use, including:

        MapFrom<TValueResolver>
        MapFrom(typeof(CustomValueResolver))
        MapFrom(aValueResolverInstance)

    In the below example, we’ll use the first option, telling AutoMapper the custom resolver type through generics:
    */

    void Main()
    {
        var configuration = new MapperConfiguration(cfg =>
            cfg.CreateMap<Source, Destination>()
                .ForMember(dest => dest.Total, opt => opt.MapFrom<CustomResolver>()));
        configuration.AssertConfigurationIsValid();
        var mapper = configuration.CreateMapper();

        var source = new Source
        {
            Value1 = 5,
            Value2 = 7
        };

        var result = mapper.Map<Source, Destination>(source);

        /*
        Although the destination member (Total) did not have any matching source member, 
        specifying a custom resolver made the configuration valid, as the resolver is now responsible for supplying a value for the destination member.
        */
    }

    /*
    If we don’t care about the source/destination types in our value resolver, or want to reuse them across maps, 
        we can just use “object” as the source/destination types:
    */
    public class MultBy2Resolver : IValueResolver<object, object, int>
    {
        public int Resolve(object source, object dest, int destMember, ResolutionContext context)
        {
            return destMember * 2;
        }
    }
}

#endregion

#region "Custom Constructor Methods"

class CustomConstructorMethods
{
    /*
https://docs.automapper.org/en/latest/Custom-value-resolvers.html#custom-constructor-methods
Custom constructor methods
*/

    /*
    Because we only supplied the type of the custom resolver to AutoMapper,
    the mapping engine will use reflection to create an instance of the value resolver.

    If we don’t want AutoMapper to use reflection to create the instance, we can supply it directly:
    */

    public class Source
    {
        public int Value1 { get; set; }
        public int Value2 { get; set; }
    }

    public class Destination
    {
        public int Total { get; set; }
    }


    public class CustomResolver : IValueResolver<Source, Destination, int>
    {
        public int Resolve(Source source, Destination destination, int member, ResolutionContext context)
        {
            return source.Value1 + source.Value2;
        }
    }

    void Test()
    {
        var configuration = new MapperConfiguration(cfg => cfg.CreateMap<Source, Destination>()
            .ForMember(dest => dest.Total,
                opt => opt.MapFrom(new CustomResolver())
            ));

        /*
        AutoMapper will use that specific object, helpful in scenarios where the resolver might have constructor arguments 
        or need to be constructed by an IoC container.

        Note that the value you return from your resolver is not simply assigned to the destination property.

        Any map that applies will be used and the result of that mapping will be the final destination property value.
        */
    }
}

#endregion

#region "Customizing the source value supplied to the resolver"

class CustomizingSourceValue
{
    /*
https://docs.automapper.org/en/latest/Custom-value-resolvers.html#customizing-the-source-value-supplied-to-the-resolver
Customizing the source value supplied to the resolver
*/

    /*
    By default, AutoMapper passes the source object to the resolver.
    This limits the reusability of resolvers, since the resolver is coupled to the source type.

    If, however, we supply a common resolver across multiple types, we configure AutoMapper to redirect the source value supplied to the resolver, 
    and also use a different resolver interface so that our resolver can get use of the source/destination members:
    */

    public class Source
    {
        public int Value1 { get; set; }
        public int Value2 { get; set; }
        public int SubTotal { get; set; }
    }

    public class Destination
    {
        public int Total { get; set; }
    }

    class OtherSource
    {
        public int OtherSubTotal { get; set; }
    }

    class OtherDest
    {
        public int OtherTotal { get; set; }
    }


    public class CustomResolver : IMemberValueResolver<object, object, decimal, decimal>
    {
        public decimal Resolve(object source, object destination, decimal sourceMember, decimal destinationMember,
            ResolutionContext context)
        {
            // logic here
            return 0;
        }
    }

    void Test()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            //cfg.CreateMap<Source, Destination>()
            //    .ForMember(dest => dest.Total,
            //        opt => opt.MapFrom<CustomResolver, decimal>(src => src.SubTotal));
            //cfg.CreateMap<OtherSource, OtherDest>()
            //    .ForMember(dest => dest.OtherTotal,
            //        opt => opt.MapFrom<CustomResolver, decimal>(src => src.OtherSubTotal));
        });
    }
}

#endregion

#region "Passing in key-value to Mapper"

class PassingInKeyValue
{
    /*
https://docs.automapper.org/en/latest/Custom-value-resolvers.html#passing-in-key-value-to-mapper
Passing in key-value to Mapper
*/

    /*
    When calling map you can pass in extra objects by using key-value and using a custom resolver to get the object from context.
    */

    public class Source
    {
        public int Value1 { get; set; }
        public int Value2 { get; set; }
        public int SubTotal { get; set; }
    }

    public class Dest
    {
        public string Foo { get; set; }
    }

    class ConfigProfile : Profile
    {
        public ConfigProfile()
        {
            /*
            This is how to setup the mapping for this custom resolver
            */
            CreateMap<Source, Dest>()
                .ForMember(dest => dest.Foo,
                    opt => opt.MapFrom((src, dest, destMember, context) => context.Items["Foo"]));
        }
    }

    void Main()
    {
        var src = new Source();
        var configuration = new MapperConfiguration(cfg => { cfg.AddProfile<ConfigProfile>(); });

        var mapper = configuration.CreateMapper();

        mapper.Map<Source, Dest>(src, opt => opt.Items["Foo"] = "Bar");
    }
}

#endregion

#endregion

#region "Conditional Mapping"

#region "Conditional Mapping"

/*
https://docs.automapper.org/en/latest/Conditional-mapping.html#conditional-mapping
Conditional Mapping
*/
/*
AutoMapper allows you to add conditions to properties that must be met before that property will be mapped.

This can be used in situations like the following where we are trying to map from an int to an unsigned int.
*/

class ConditionalMapping
{
    class Foo
    {
        public int baz;
    }

    class Bar
    {
        public uint baz;
    }

    /*
    In the following mapping the property baz will only be mapped if it is greater than or equal to 0 in the source object.
    */

    void Test()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Foo, Bar>()
                .ForMember(dest => dest.baz, opt => opt.Condition(src => (src.baz >= 0)));
        });
    }
}

#endregion

#region "Preconditions"

class Preconditions
{
    /*
https://docs.automapper.org/en/latest/Conditional-mapping.html#preconditions
Preconditions
*/

    /*
    Similarly, there is a PreCondition method.
    The difference is that it runs sooner in the mapping process, before the source value is resolved (think MapFrom). 

    So the precondition is called, then we decide which will be the source of the mapping (resolving), 
    then the condition is called and finally the destination value is assigned.
    */

    class Foo
    {
        public int baz;
    }

    class Bar
    {
        public uint baz;
    }

    void Main()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Foo, Bar>()
                .ForMember(dest => dest.baz, opt =>
                {
                    opt.PreCondition(src => (src.baz >= 0));
                    //opt.MapFrom(src =>
                    //{
                    //    // Expensive resolution process that can be avoided with a PreCondition
                    //});
                });
        });
    }
}

#endregion

#endregion

#region "Converters and Resolvers Syntax"

/*
https://docs.automapper.org/en/latest/Value-converters.html#value-converters
Converters Syntax
*/
/*
In simplified syntax:

    Type converter = Func<TSource, TDestination, TDestination>
    Value resolver = Func<TSource, TDestination, TDestinationMember>
    Member value resolver = Func<TSource, TDestination, TSourceMember, TDestinationMember>
    Value converter = Func<TSourceMember, TDestinationMember>

*/

#endregion

#region "Null Substitution"

class NullSubstitution
{
    /*
https://docs.automapper.org/en/latest/Null-substitution.html#null-substitution
Null Substitution
*/

    /*
    Null substitution allows you to supply an alternate value for a destination member if the source value is null anywhere along the member chain.
    This means that instead of mapping from null, it will map from the value you supply.
    */

    class Source
    {
        public string Value { get; set; }
    }

    class Dest
    {
        public string Value { get; set; }
    }

    void Test()
    {
        var config = new MapperConfiguration(cfg => cfg.CreateMap<Source, Dest>()
            .ForMember(destination => destination.Value, opt => opt.NullSubstitute("Other Value")));

        var source = new Source {Value = null};
        var mapper = config.CreateMapper();
        var dest = mapper.Map<Source, Dest>(source);


        source.Value = "Not null";

        dest = mapper.Map<Source, Dest>(source);
    }
}

#endregion

#region "Value Converters"

class ValueConverters
{
    /*
https://docs.automapper.org/en/latest/Value-converters.html#value-converters
Value Converters
*/

    /*
    Value converters are a cross between Type Converters and Value Resolvers. 

    Type converters are globally scoped, so that any time you map from type Foo to type Bar in any mapping, the type converter will be used.

    Value converters are scoped to a single map, and receive the source and destination objects to resolve to a value to map to the destination member.

    Optionally value converters can receive the source member as well.

    To configure a value converter, use at the member level:
    */

    public class CurrencyFormatter : IValueConverter<decimal, string>
    {
        public string Convert(decimal source, ResolutionContext context)
            => source.ToString("c");
    }

    class Order
    {
        public int OrderAmount { get; set; }
    }

    class OrderDto
    {
        public string Amount { get; set; }
    }

    class OrderLineItem
    {
        public int LITotal { get; set; }
    }

    class OrderLineItemDto
    {
        public string Total { get; set; }
    }

    void Main()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>()
                .ForMember(d => d.Amount, opt => opt.ConvertUsing(new CurrencyFormatter(), src => src.OrderAmount));
            cfg.CreateMap<OrderLineItem, OrderLineItemDto>()
                .ForMember(d => d.Total, opt => opt.ConvertUsing(new CurrencyFormatter(), src => src.LITotal));
        });

        /*
        If you need the value converters instantiated by the service locator, you can specify the type instead:
        */
        var configuration1 = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Order, OrderDto>()
                .ForMember(d => d.Amount, opt => opt.ConvertUsing<CurrencyFormatter, decimal>());
            cfg.CreateMap<OrderLineItem, OrderLineItemDto>()
                .ForMember(d => d.Total, opt => opt.ConvertUsing<CurrencyFormatter, decimal>());
        });

        /*
        If you do not know the types or member names at runtime, 
        use the various overloads that accept System.Type and string-based members:
        */
        var configuration2 = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap(typeof(Order), typeof(OrderDto))
                .ForMember("Amount", opt => opt.ConvertUsing(new CurrencyFormatter(), "OrderAmount"));
            cfg.CreateMap(typeof(OrderLineItem), typeof(OrderLineItemDto))
                .ForMember("Total", opt => opt.ConvertUsing(new CurrencyFormatter(), "LITotal"));
        });
    }
}

#endregion

#region "Value Transformers"

/*
https://docs.automapper.org/en/latest/Value-transformers.html#value-transformers
Value Transformers
*/
/*
Value transformers apply an additional transformation to a single type. 
Before assigning the value, AutoMapper will check to see if the value to be set has any value transformations associated, 
and will apply them before setting.

You can create value transformers at several different levels:

    Globally
    Profile
    Map
    Member

*/

class ValueTransformers
{
    class Source
    {
        public string Value { get; set; }
    }

    class Dest
    {
        public string Value { get; set; }
    }

    void Main()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.ValueTransformers.Add<string>(val => val + "!!!");
            cfg.CreateMap<Source, Dest>();
        });

        var source = new Source {Value = "Hello"};
        var mapper = configuration.CreateMapper();
        var dest = mapper.Map<Dest>(source);
    }
}

#endregion