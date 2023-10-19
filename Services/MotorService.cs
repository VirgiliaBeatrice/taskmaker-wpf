using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Entity;
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Services
{
    public class MotorService : BaseEntityManager<MotorEntity>
    {
        private readonly SerialService serialService;
        public MotorService(SerialService serialService) {
            this.serialService = serialService;
        }

        public override bool Update(MotorEntity entity) {
            serialService.Update(entity.NuibotBoardId, entity.NuibotMotorId, entity.Value[0]);

            return base.Update(entity);
        }
    }
}
