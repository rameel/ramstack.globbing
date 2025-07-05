using System.Runtime.Intrinsics.X86;

namespace Ramstack.Globbing;

[TestFixture]
public class SimdConfigurationTests
{
    [Test]
    public void VerifySimdConfiguration()
    {
        var isAvx2Disabled = Environment.GetEnvironmentVariable("COMPlus_EnableAVX2") == "0";
        var isSse2Disabled = Environment.GetEnvironmentVariable("COMPlus_EnableSSE2") == "0";

        Assert.That(isAvx2Disabled, Is.EqualTo(!Avx2.IsSupported));
        Assert.That(isSse2Disabled, Is.EqualTo(!Sse2.IsSupported));
    }
}
