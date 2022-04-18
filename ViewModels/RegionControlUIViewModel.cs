using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Services;

namespace taskmaker_wpf.ViewModels {
    public interface IPage {
    
    }

    public class RegionControlUIViewModel : BindableBase {

        private SimplexService _simplexService;
        private IPage _page;

        public RegionControlUIViewModel(SimplexService service) {
            _simplexService = service;
        }

        private void InitializeCanvasPage() {
            _page = null;
        }
    }
}
