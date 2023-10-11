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

        public MotorService(MotorInteractorBus motorBus) {
            _motorBus = motorBus;
        }

        public MotorEntity AddMotor() {
            var req = new AddMotorRequest();

            _motorBus.Handle(req, out MotorEntity output);

            return output;
        }

        public void RemoveMotor(MotorEntity input) {
            var req = new DeleteMotorRequest {
                Id = input.Id
            };

            _motorBus.Handle(req, out bool output);
        }

        public MotorEntity UpdateMotor(MotorEntity input) {
            var req = new UpdateMotorRequest {
                Id = input.Id,
                Value = input
            };

            _motorBus.Handle(req, out MotorEntity output);

            return output;
        }
        public MotorEntity UpdateMotorValue(int id, double newValue) {
            var req = new UpdateMotorRequest {
                Id = id,
                PropertyName = "MotorValue",
                Value = new double[] { newValue },
            };

            _motorBus.Handle(req, out MotorEntity motor);

            return motor;
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

            var motors = InvalidateMotors();

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

            var idx = 0;
            foreach (var motor in motors) {
                motor.Max = 10000;
                motor.Min = -10000;
                idx++;

                UpdateMotor(motor);
            }
        }

    }
}
