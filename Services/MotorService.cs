using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using taskmaker_wpf.Domain;
using taskmaker_wpf.ViewModels;

namespace taskmaker_wpf.Services
{
    public class MotorService
    {
        private readonly MotorInteractorBus _motorBus;

        public List<MotorEntity> Motors { get; set; } = new List<MotorEntity>();

        public MotorService(MotorInteractorBus motorBus) {
            _motorBus = motorBus;
        }

        public MotorEntity AddMotor() {
            var req = new AddMotorRequest();

            _motorBus.Handle(req, out MotorEntity output);
            Motors.Add(output);

            return output;
        }

        public void RemoveMotor(MotorEntity input) {
            var req = new DeleteMotorRequest {
                Id = input.Id
            };

            _motorBus.Handle(req, out bool output);
            Motors.Remove(input);
        }

        public void UpdateMotor(ref MotorEntity motor) {
            var req = new UpdateMotorRequest {
                Id = motor.Id,
                Value = motor
            };

            _motorBus.Handle(req, out motor);
        }
        public void UpdateMotorValue(ref MotorEntity motor, double newValue) {
            var req = new UpdateMotorRequest {
                Id = motor.Id,
                PropertyName = "MotorValue",
                Value = new double[] { newValue },
            };

            _motorBus.Handle(req, out motor);
        }

        public MotorEntity[] InvalidateMotors() {
            _motorBus.Handle(new ListMotorRequest(), out MotorEntity[] motors);

            return motors;
        }

        public void Initialize() {
            AddMotor();
            AddMotor();
            AddMotor();
            AddMotor();
            AddMotor();
            AddMotor();

            var motors = Motors;

            motors[0].NuibotBoardId = 0;
            motors[0].NuibotMotorId = 0;
            motors[1].NuibotBoardId = 0;
            motors[1].NuibotMotorId = 1;
            motors[2].NuibotBoardId = 0;
            motors[2].NuibotMotorId = 2;
            motors[3].NuibotBoardId = 1;
            motors[3].NuibotMotorId = 0;
            motors[4].NuibotBoardId = 1;
            motors[4].NuibotMotorId = 1;
            motors[5].NuibotBoardId = 1;
            motors[5].NuibotMotorId = 2;


            for (int i = 0; i < motors.Count(); i++) {
                var motor = motors[i];
                motor.Max = 10000;
                motor.Min = -10000;

                UpdateMotor(ref motor);
            }
        }

    }
}
