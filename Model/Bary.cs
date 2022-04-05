using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Numpy;

namespace taskmaker_wpf.Model.Data {
    public interface IBary {
        NDarray Indices { get; set; }
        NDarray Zero { get; }
        NDarray GetLambdas(NDarray p, params object[] args);
    }

    public class SimplexBaryD : IBary, IDisposable {
        private bool disposedValue;

        public NDarray Basis { get; set; }
        public NDarray Indices { get; set; }
        public NDarray A { get; private set; }

        public SimplexBaryD(NDarray basis) {
            Basis = np.asanyarray(basis).T;
            
            using(var affineFactor = np.ones(Basis.shape[1])) {
                if (Basis.shape[1] == 2)
                    A = basis;
                else
                    A = np.vstack(affineFactor, Basis);
            }
        }

        public NDarray GetLambdas(NDarray b, params object[] args) {
            var B = np.hstack(np.ones(1), b);

            if (Basis.shape[1] == 2) {
                B.Dispose();
                B = b;
            }

            var x = np.linalg.solve(A, B);

            if (args.Length == 0)
                return x;
            else {
                var newResult = np.zeros((int)args[0]);

                for (var i = 0; i < x.shape[0]; i++) {
                    newResult[$"{Indices[i] - 1}"] = x[i];
                }

                x.Dispose();

                return newResult;
            }
        }

        public NDarray Zero => np.zeros(Basis.shape[0], 1);

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                    Basis.Dispose();
                    A.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SimplexBaryD()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class VoronoiBaryD : IBary, IDisposable {
        private bool disposedValue;

        public SimplexBaryD[] GovernorBarys { get; set; }
        public Func<NDarray, NDarray> GetFactors;

        public VoronoiBaryD(SimplexBaryD[] barys) {
            GovernorBarys = barys;
        }

        public NDarray Zero => GovernorBarys[0].Zero;

        public NDarray Indices { get; set; }

        public NDarray GetLambdas(NDarray p, params object[] args) {
            if (GovernorBarys.Length == 1) {
                return GovernorBarys[0].GetLambdas(p, args);
            }
            else {
                var factors = GetFactors(p);
                var lambda0 = factors[0] * GovernorBarys[0].GetLambdas(p, args);
                var lambda1 = factors[1] * GovernorBarys[1].GetLambdas(p, args);

                return lambda0 + lambda1;
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~VornoiBaryD()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class ComplexBaryD : IDisposable {
        private bool disposedValue;

        public NDarray Basis { get; set; }
        public IBary[] Barys { get; set; }

        public ComplexBaryD(NDarray basis, IBary[] barys) {
            Basis = basis;
            Barys = barys;
        }

        public NDarray GetLambdas(IBary target, NDarray p) {
            return target.GetLambdas(p, Basis.shape[0]);
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ComplexBary()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class NLinearMap : IDisposable {
        public ComplexBaryD[] Barys { get; set; }

        private NDarray _wTensor;
        private int[] _shape;
        private bool disposedValue;

        private bool _isSet => !bool.Parse(np.isnan(_wTensor.sum()).repr);

        public NLinearMap(ComplexBaryD[] barys, int targetDim) {
            Barys = barys;

            // Shape: 2 - BiLinear, n -  
            var dim = new int[] { targetDim, Barys.Length };
            _shape = dim.Concat(Barys.Select(e => e.Basis.shape[0])).ToArray();
            //_shape = dim.Concat(Barys.Select(e => e.Basis.shape[0])).Concat(new int[] { targetDim }).ToArray();

            _wTensor = np.empty(_shape);
            _wTensor.fill(np.nan);
        }

        public void SetValue(int[] indices, NDarray value) {
            _wTensor[$":,:,{indices[0]}"] = np.atleast_2d(value).T;
        }

        public NDarray MapTo(NDarray lambdas) {
            if (!_isSet)
                return null;

            NDarray kronProd = null;

            for (var i = 0; i < lambdas.shape[0]; i++) {
                if (i == 0)
                    kronProd = np.array(lambdas[$"{i},:"]);
                else
                    kronProd = np.kron(kronProd, lambdas[$"{i},:"]);
            }

            var w = np.dot(_wTensor.reshape(_shape[0], -1), kronProd);

            return w;
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~NLinearMap()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
