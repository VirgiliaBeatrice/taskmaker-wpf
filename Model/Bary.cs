using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Numpy;

namespace taskmaker_wpf.Model.Data {
    public interface IBary {
        //NDarray Indices { get; set; }
        NodeM[] BasisRef { get; }
        NDarray Zero { get; }
        NDarray GetLambdas(NDarray p, params object[] args);
    }

    public class SimplexBaryD : IBary, IDisposable {
        private bool disposedValue;

        private NodeM[] _nodeRefs;
        private NDarray _basis;

        public NodeM[] BasisRef => _nodeRefs;
        //public NDarray Indices { get; set; }
        public NDarray A { get; private set; }

        public SimplexBaryD(IEnumerable<NodeM> refs) {
            _nodeRefs = refs.ToArray();
            _basis = np.array(_nodeRefs.Select(e => e.Location).ToArray()).T;

            using (var affineFactor = np.ones(_basis.shape[1])) {
                if (_basis.shape[1] == 2)
                    A = _basis;
                else
                    A = np.vstack(affineFactor, _basis);
            }
        }

        public NDarray GetLambdas(NDarray b, params object[] args) {
            var B = _basis.shape[1] == 2 ? b : np.hstack(np.ones(1), b);

            var x = np.linalg.solve(A, B);

            if (args.Length == 0)
                return np.atleast_2d(x);
            else {
                var length = (int)args[0];
                var indices = (int[])args[1];
                var newX = np.zeros(length);

                for (var i = 0; i < x.shape[0]; i++) {
                    newX[$"{indices[i]}"] = x[i];
                }

                x.Dispose();

                return np.atleast_2d(newX);
            }
        }

        public NDarray Zero => np.atleast_2d(np.zeros(_basis.shape[0]));

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: dispose managed state (managed objects)
                    _basis.Dispose();
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

        public NodeM[] BasisRef => GovernorBarys.SelectMany(e => e.BasisRef).Distinct().ToArray();

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

    public class ComplexBaryD {
        public int BasisDim => _basis.Length;
        private NodeM[] _basis;

        public ComplexBaryD(IEnumerable<NodeM> basis) {
            _basis = basis.ToArray();
        }


        public NDarray GetLambdas(IBary target, NDarray p) {
            var indices = target.BasisRef.Select(e => _basis.ToList().IndexOf(e)).ToArray();

            return target.GetLambdas(p, _basis.Length, indices);
        }
    }

    public class NLinearMap : IDisposable {
        public ComplexBaryD[] Barys { get; set; }

        private NDarray _wTensor;
        private int[] _shape;
        private bool disposedValue;

        private bool _isSet => !bool.Parse(np.isnan(_wTensor.sum()).repr);


        /// <summary>
        /// NLinearMap
        /// [ targetDim , ...(BasisDims) ]
        /// targetDim = dimension of target's collection 
        /// ...(BasisDims) = spreading of basis dimension of each barys
        /// </summary>
        /// <param name="barys"></param>
        /// <param name="targetDim"></param>
        public NLinearMap(ComplexBaryD[] barys, int targetDim) {
            Barys = barys;

            // Shape: 2 - BiLinear, n -  
            var partialDim = new int[] { targetDim };
            _shape = partialDim
                .Concat(Barys
                    .Select(e => e.BasisDim))
                .ToArray();

            _wTensor = np.empty(_shape);
            _wTensor.fill(np.nan);
        }

        public void SetValue(int[] indices, NDarray value) {
            // only 1 bary
            _wTensor[$":,{indices[0]}"] = np.atleast_2d(value);
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
