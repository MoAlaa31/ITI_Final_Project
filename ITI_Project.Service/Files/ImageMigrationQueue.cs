using ITI_Project.Core.IServices;
using System.Threading.Channels;

namespace ITI_Project.Services.Files
{

    public class ImageMigrationQueue : IImageMigrationQueue
    {
        private readonly Channel<bool> queue = Channel.CreateUnbounded<bool>();

        public void QueueMigration()
        {
            queue.Writer.TryWrite(true);
        }

        public async Task<bool> WaitForMigrationAsync(CancellationToken cancellationToken)
        {
            return await queue.Reader.ReadAsync(cancellationToken);
        }
    }
}
