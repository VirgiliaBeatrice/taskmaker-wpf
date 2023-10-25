using NLog;
using taskmaker_wpf.Entity;

namespace taskmaker_wpf.Services
{
    public class SessionService : BaseEntityManager<SessionEntity> {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly MotorService _motorSrv;
        private readonly UIService _uiSrv;

        public SessionService(MotorService motorSrv, UIService uiSrv) {
            _motorSrv = motorSrv;
            _uiSrv = uiSrv;
        }
    }
}
