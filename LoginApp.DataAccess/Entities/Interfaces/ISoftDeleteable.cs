using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginApp.DataAccess.Entities.Interfaces
{
    public interface ISoftDeleteable
    {
        bool IsDeleted { get; set; }

        void Delete();
        void UndoDelete();
    }
}
