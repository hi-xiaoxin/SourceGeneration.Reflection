using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SourceGeneration.Reflection.Test;

[TestClass]
public class CreateInstanceValueTypeTest
{
    [TestMethod]
    public void Create_With_ParamaterlessCtor()
    {
        var type = SourceReflector.GetRequiredType<CreateInstanceTestStruct_ParamaterlessCtor>();
        var instance = SourceReflector.CreateInstance(type);

        Assert.IsNotNull(instance);
        Assert.IsInstanceOfType(instance, typeof(CreateInstanceTestStruct_ParamaterlessCtor));
        Assert.AreEqual("Default", ((CreateInstanceTestStruct_ParamaterlessCtor)instance).Value);
    }

    [TestMethod]
    public void Create_With_DefaultCtor()
    {
        var type = SourceReflector.GetRequiredType<CreateInstanceTestStruct_DefaultCtor>();
        var instance = SourceReflector.CreateInstance(type);

        Assert.IsNotNull(instance);
        Assert.IsInstanceOfType(instance, typeof(CreateInstanceTestStruct_DefaultCtor));
        Assert.IsNull(((CreateInstanceTestStruct_DefaultCtor)instance).Value);
    }

    [TestMethod]
    public void Create_With_ParameterCtor_DefaultInvoke()
    {
        var type = SourceReflector.GetRequiredType<CreateInstanceTestStruct_ParamatersCtor>();
        var instance = SourceReflector.CreateInstance(type);

        Assert.IsNotNull(instance);
        Assert.IsInstanceOfType(instance, typeof(CreateInstanceTestStruct_ParamatersCtor));
        Assert.AreEqual("Default", ((CreateInstanceTestStruct_ParamatersCtor)instance).Value);
    }

    [TestMethod]
    public void Create_With_ParameterCtor_ArgumentInvoke()
    {
        var type = SourceReflector.GetRequiredType<CreateInstanceTestStruct_ParamatersCtor>();
        var instance = SourceReflector.CreateInstance(type, "Test");

        Assert.IsNotNull(instance);
        Assert.IsInstanceOfType(instance, typeof(CreateInstanceTestStruct_ParamatersCtor));
        Assert.AreEqual("Test", ((CreateInstanceTestStruct_ParamatersCtor)instance).Value);
    }


    [TestMethod]
    public void ReflectionCreate_With_DefaultCtor()
    {
        var type = SourceReflector.GetRequiredType<ReflectionCreateInstanceTestStruct_DefaultCtor>(true);
        var instance = SourceReflector.CreateInstance(type);

        Assert.IsNotNull(instance);
        Assert.IsInstanceOfType(instance, typeof(ReflectionCreateInstanceTestStruct_DefaultCtor));
        Assert.IsNull(((ReflectionCreateInstanceTestStruct_DefaultCtor)instance).Value);
    }

    [TestMethod]
    public void ReflectionCreate_With_ParamaterlessCtor()
    {
        var type = SourceReflector.GetRequiredType<ReflectionCreateInstanceTestStruct_ParamaterlessCtor>(true);
        var instance = SourceReflector.CreateInstance(type);

        Assert.IsNotNull(instance);
        Assert.IsInstanceOfType(instance, typeof(ReflectionCreateInstanceTestStruct_ParamaterlessCtor));
        Assert.AreEqual("Default", ((ReflectionCreateInstanceTestStruct_ParamaterlessCtor)instance).Value);
    }

    [TestMethod]
    public void ReflectionCreate_With_ParameterCtor_ArgumentInvoke()
    {
        var type = SourceReflector.GetRequiredType<ReflectionCreateInstanceTestStruct_ParamatersCtor>(true);
        var instance = SourceReflector.CreateInstance(type, "Test");

        Assert.IsNotNull(instance);
        Assert.IsInstanceOfType(instance, typeof(ReflectionCreateInstanceTestStruct_ParamatersCtor));
        Assert.AreEqual("Test", ((ReflectionCreateInstanceTestStruct_ParamatersCtor)instance).Value);
    }


    [TestMethod]
    public void ReflectionCreate_With_ParameterCtor_DefaultInvoke()
    {
        var type = SourceReflector.GetRequiredType<ReflectionCreateInstanceTestStruct_ParamatersCtor>(true);
        var instance = SourceReflector.CreateInstance(type);

        Assert.IsNotNull(instance);
        Assert.IsInstanceOfType(instance, typeof(ReflectionCreateInstanceTestStruct_ParamatersCtor));
        Assert.AreEqual("Default", ((ReflectionCreateInstanceTestStruct_ParamatersCtor)instance).Value);
    }

}

[SourceReflection]
public struct CreateInstanceTestStruct_ParamatersCtor
{
    public CreateInstanceTestStruct_ParamatersCtor()
    {
        Value = "Default";
    }

    public CreateInstanceTestStruct_ParamatersCtor(string value)
    {
        Value = value;
    }

    public string Value { get; set; }
}

[SourceReflection]
public struct CreateInstanceTestStruct_ParamaterlessCtor
{
    public CreateInstanceTestStruct_ParamaterlessCtor()
    {
        Value = "Default";
    }

    public string Value { get; set; }
}

[SourceReflection]
public struct CreateInstanceTestStruct_DefaultCtor
{
    public string Value { get; set; }
}

public struct ReflectionCreateInstanceTestStruct_DefaultCtor
{
    public string Value { get; set; }
}

public struct ReflectionCreateInstanceTestStruct_ParamaterlessCtor
{
    public ReflectionCreateInstanceTestStruct_ParamaterlessCtor()
    {
        Value = "Default";
    }

    public string Value { get; set; }
}

public struct ReflectionCreateInstanceTestStruct_ParamatersCtor
{
    public ReflectionCreateInstanceTestStruct_ParamatersCtor()
    {
        Value = "Default";
    }

    public ReflectionCreateInstanceTestStruct_ParamatersCtor(string value)
    {
        Value = value;
    }

    public string Value { get; set; }
}
