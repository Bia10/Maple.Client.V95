namespace Maple.Client.V95.Test;

public sealed class StructFieldRegistryTests
{
    [Test]
    public async Task Registry_TryGetField_ReturnsKnownV95Offset()
    {
        bool found = ClientStructs.Registry.TryGetField(
            nameof(ClientStructs.Fields.CWvsContext),
            "WorldId",
            out StructField field
        );

        await Assert.That(found).IsTrue();
        await Assert.That(field.StructName).IsEqualTo(nameof(ClientStructs.Fields.CWvsContext));
        await Assert.That(field.FieldName).IsEqualTo("WorldId");
        await Assert.That(field.Offset).IsEqualTo(ClientStructs.Offsets.CWvsContext.WorldId);
    }

    [Test]
    public async Task Registry_GetFields_ReturnsExpectedFieldCountForCLogin()
    {
        IReadOnlyDictionary<string, StructField> fields = ClientStructs.Registry.GetFields(
            nameof(ClientStructs.Fields.CLogin)
        );

        await Assert.That(fields.Count).IsEqualTo(ClientStructs.Fields.CLogin.Count);
        await Assert.That(fields.ContainsKey("LoginStep")).IsTrue();
        await Assert.That(fields.ContainsKey("CharSelected")).IsTrue();
    }

    [Test]
    public async Task Registry_Constructor_WhenDuplicateFieldExists_Throws()
    {
        StructField duplicate = new(nameof(ClientStructs.Fields.CQuestMan), "WorldId", 0x64);

        await Assert
            .That(() =>
                new StructFieldRegistry([
                    [duplicate, duplicate],
                ])
            )
            .Throws<ArgumentException>();
    }
}
