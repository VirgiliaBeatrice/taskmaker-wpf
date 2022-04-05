using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Numpy;

namespace taskmaker_wpf.Model.SimplicialMapping {
    public abstract class VoronoiRegion {
        public abstract NDarray GetLambdas(NDarray b, params float[] args);
    }

    public class VoronoiRegion_Rect : VoronoiRegion {
        public Simplex Governor;

        public override NDarray GetLambdas(NDarray b, params float[] args) {
            return Governor.GetLambdas(b);
        }
    }

    public class VoronoiRegion_Sector : VoronoiRegion {
        public Simplex[] Governors;

        public override NDarray GetLambdas(NDarray b, params float[] args) {
            var lambda0 = Governors[0].GetLambdas(b);
            var lambda1 = Governors[1].GetLambdas(b);
            var factor = args[0];

            return factor * lambda0 + (1.0f - factor) * lambda1;
        }
    }

    public class Exterior {
        public VoronoiRegion[] Regions { get; set; }

        public Exterior(IEnumerable<VoronoiRegion> regions) {
            Regions = regions.ToArray();
        }
    }

    public class Interior {
        public Simplex[] Regions;

        public Interior(IEnumerable<Simplex> regions) {
            Regions = regions.ToArray();
        }
    }


    /// <summary>
    /// Simplex object.
    /// Dim() = [?=2, 3]
    /// </summary>
    public class Simplex {
        public NDarray Basis { get; set; }
        public int Dimension => Basis.shape[1];

        private NDarray _mat_a;

        public Simplex(NDarray basis) {
            Basis = basis;

            var affineFactor = np.ones(Dimension);

            if (Dimension == 2) {
                _mat_a = Basis;
            }
            else {
                _mat_a = np.vstack(affineFactor, Basis);
            }
        }

        public NDarray GetLambdas(NDarray b, bool isZero = false) {
            if (isZero) {
                return np.zeros(Dimension);
            }
            else {
                NDarray B;

                if (Dimension == 2) {
                    B = b;
                }
                else {
                    B = np.hstack(np.ones(1), b);
                }

                return np.linalg.solve(_mat_a, B);
            }
        }
    }

    /// <summary>
    /// Simplicial complex
    /// </summary>
    public class Complex {
        public Interior Interior { get; set; }
        public Exterior Exterior { get; set; }

        // TODO
        public NDarray GetLambdas(NDarray b) {
            return np.zeros(1, 2);
        }
    }
}
