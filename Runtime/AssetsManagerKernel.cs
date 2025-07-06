namespace MK.AssetsManager
{
    using MK.Kernel;

    public static class AssetsManagerKernel
    {
        public static void AssetsManagerConfigure(this IBuilder builder)
        {
            builder.Add<AddressableManager>().AsImplementedInterface();
            builder.Add<ExternalAssetManager>().AsImplementedInterface();
        }
    }
}