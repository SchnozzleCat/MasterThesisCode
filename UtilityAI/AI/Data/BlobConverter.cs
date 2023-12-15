using Unity.Collections;
using Unity.Entities;

namespace Schnozzle.AI.Data
{
    public static class BlobConverter<T>
        where T : IAspect
    {

        // public BlobAssetReference<> Convert(Allocator allocator = Allocator.Persistent)
        // {
        //     using var builder = new BlobBuilder(allocator);
        //
        //     builder.CreateBlobAssetReference<>()
        // }
    }
}
