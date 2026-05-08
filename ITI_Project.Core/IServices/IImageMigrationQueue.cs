using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITI_Project.Core.IServices
{
    public interface IImageMigrationQueue
    {
        void QueueMigration();

        Task<bool> WaitForMigrationAsync(
            CancellationToken cancellationToken);
    }
}
