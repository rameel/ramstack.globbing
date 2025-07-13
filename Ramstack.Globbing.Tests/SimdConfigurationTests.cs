using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;

namespace Ramstack.Globbing;

[TestFixture]
public class SimdConfigurationTests
{
    [Test]
    public void VerifySimdConfiguration()
    {
        switch (RuntimeInformation.ProcessArchitecture)
        {
            case Architecture.X64:
                var isSse2Disabled = Environment.GetEnvironmentVariable("COMPlus_EnableSSE2") == "0";
                var isSse41Disabled = Environment.GetEnvironmentVariable("COMPlus_EnableSSE41") == "0";
                var isAvx2Disabled = Environment.GetEnvironmentVariable("COMPlus_EnableAVX2") == "0";

                Assert.That(isSse2Disabled, Is.EqualTo(!Sse2.IsSupported));
                Assert.That(isSse41Disabled, Is.EqualTo(!Sse41.IsSupported));
                Assert.That(isAvx2Disabled, Is.EqualTo(!Avx2.IsSupported));
                break;

            case Architecture.Arm64:
                var isAdvSimdDisabled = Environment.GetEnvironmentVariable("COMPlus_EnableAdvSimd") == "0";

                Assert.That(isAdvSimdDisabled, Is.EqualTo(!AdvSimd.IsSupported));
                break;
        }
    }
}
