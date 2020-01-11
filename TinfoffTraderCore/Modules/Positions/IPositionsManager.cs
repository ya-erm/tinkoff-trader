using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TinkoffTraderCore.Models;

namespace TinkoffTraderCore.Modules.Positions
{
    /// <summary>
    /// Интерфейс менеджера позиций
    /// </summary>
    public interface IPositionsManager
    {
        /// <summary>
        /// Список позиций
        /// </summary> 
        ObservableCollection<Position> Positions { get; }

        /// <summary>
        /// Получить актуальные позиции и обновить список
        /// </summary>
        /// <returns></returns>
        Task LoadPositionsAsync();
    }
}
