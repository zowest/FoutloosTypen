using FoutloosTypen.Core.Models;
using System.Collections.Generic;

namespace FoutloosTypen.Core.Interfaces.Repositories
{
    public interface ISharePostRepository
    {
        SharePost Add(SharePost post);
        IEnumerable<SharePost> GetAll();
    }
}
