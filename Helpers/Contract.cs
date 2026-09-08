using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class IgnoreUnchangedDefaultsResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        JsonProperty property = base.CreateProperty(member, memberSerialization);

        if (property.Readable && member.DeclaringType != null && !member.DeclaringType.IsValueType)
        {
            property.ShouldSerialize = instance =>
            {
                try
                {
                    object defaultObject = Activator.CreateInstance(member.DeclaringType);
                    
                    object currentValue = property.ValueProvider.GetValue(instance);
                    object defaultValue = property.ValueProvider.GetValue(defaultObject);

                    if (currentValue == null && defaultValue == null) return false;
                    if (currentValue == null || defaultValue == null) return true;
                    
                    return !currentValue.Equals(defaultValue);
                }
                catch
                {
                    return true; 
                }
            };
        }

        return property;
    }
}
