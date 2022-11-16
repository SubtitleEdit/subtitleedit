using iTin.Hardware.Specification.Cpuid;
using iTin.Hardware.Specification;

namespace Nikse.SubtitleEdit.Logic
{
    public static class CpuInfo
    {
        public static bool HasAvx2()
        {
            try
            {
                var avx2 = CPUID.Instance.Leafs.GetProperty(LeafProperty.ExtendedFeatures.AVX2);
                return avx2.Success && (bool)avx2.Result.Value;
            }
            catch
            {
                return false;
            }
        }
    }
}
