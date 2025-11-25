using DRCore.Enums;
using DRCore.Models;

namespace DRCore.Interfaces
{
    public interface ICFDI
    {
        Task<DRResult<string>> Timbrar(ObjTypeEnum objType, int docEntry);
    }
}
