using System.Collections.Generic;

namespace taskmaker_wpf.Entity
{
    public class SessionEntity : BaseEntity
    {
        public List<ControlUiEntity> Uis { get; set; } = new List<ControlUiEntity>();
        public NLinearMapEntity Map { get; set; }
    }
}
