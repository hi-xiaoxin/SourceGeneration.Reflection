namespace SourceGeneration.Reflection.Test;

[TestClass]
public class EnumTest
{
    [TestMethod]
    public void Enum()
    {
        var type = SourceReflector.GetType<EnumTestObject>(true);

        Assert.IsNotNull(type);

        Assert.AreEqual(2, type.DeclaredFields.Length);
        Assert.AreEqual("A", type.DeclaredFields[0].Name);
        Assert.AreEqual("B", type.DeclaredFields[1].Name);

        Assert.AreEqual(EnumTestObject.A, type.DeclaredFields[0].GetValue(null));
        Assert.AreEqual(EnumTestObject.B, type.DeclaredFields[1].GetValue(null));
    }
}

[SourceReflection]
public enum EnumTestObject
{
    A = 21,
    B = 49,
}
