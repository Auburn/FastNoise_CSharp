// FastNoise.cs
//
// MIT License
//
// Copyright(c) 2017 Jordan Peck
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// The developer's email is jorzixdan.me2@gzixmail.com (for great email, take
// off every 'zix'.)
//

// Uncomment the line below to swap all the inputs/outputs/calculations of FastNoise to doubles instead of floats
//#define FN_USE_DOUBLES

#if FN_USE_DOUBLES
using FN_DECIMAL = System.Double;
#else
using FN_DECIMAL = System.Single;
#endif

using System;


namespace FastNoise {
	public partial class FastNoise {
		// Simplex Noise
		public FN_DECIMAL GetSimplexFractal( FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
			x *= m_frequency;
			y *= m_frequency;
			z *= m_frequency;

			switch( m_fractalType ) {
			case FractalType.FBM:
				return SingleSimplexFractalFBM( x, y, z );
			case FractalType.Billow:
				return SingleSimplexFractalBillow( x, y, z );
			case FractalType.RigidMulti:
				return SingleSimplexFractalRigidMulti( x, y, z );
			default:
				return 0;
			}
		}

		private FN_DECIMAL SingleSimplexFractalFBM( FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
			int seed = m_seed;
			FN_DECIMAL sum = SingleSimplex( seed, x, y, z );
			FN_DECIMAL amp = 1;

			for( int i = 1; i < m_octaves; i++ ) {
				x *= m_lacunarity;
				y *= m_lacunarity;
				z *= m_lacunarity;

				amp *= m_gain;
				sum += SingleSimplex( ++seed, x, y, z ) * amp;
			}

			return sum * m_fractalBounding;
		}

		private FN_DECIMAL SingleSimplexFractalBillow( FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
			int seed = m_seed;
			FN_DECIMAL sum = Math.Abs( SingleSimplex( seed, x, y, z ) ) * 2 - 1;
			FN_DECIMAL amp = 1;

			for( int i = 1; i < m_octaves; i++ ) {
				x *= m_lacunarity;
				y *= m_lacunarity;
				z *= m_lacunarity;

				amp *= m_gain;
				sum += ( Math.Abs( SingleSimplex( ++seed, x, y, z ) ) * 2 - 1 ) * amp;
			}

			return sum * m_fractalBounding;
		}

		private FN_DECIMAL SingleSimplexFractalRigidMulti( FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
			int seed = m_seed;
			FN_DECIMAL sum = 1 - Math.Abs( SingleSimplex( seed, x, y, z ) );
			FN_DECIMAL amp = 1;

			for( int i = 1; i < m_octaves; i++ ) {
				x *= m_lacunarity;
				y *= m_lacunarity;
				z *= m_lacunarity;

				amp *= m_gain;
				sum -= ( 1 - Math.Abs( SingleSimplex( ++seed, x, y, z ) ) ) * amp;
			}

			return sum;
		}

		public FN_DECIMAL GetSimplex( FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
			return SingleSimplex( m_seed, x * m_frequency, y * m_frequency, z * m_frequency );
		}

		private const FN_DECIMAL F3 = (FN_DECIMAL)( 1.0 / 3.0 );
		private const FN_DECIMAL G3 = (FN_DECIMAL)( 1.0 / 6.0 );
		private const FN_DECIMAL G33 = G3 * 3 - 1;

		private FN_DECIMAL SingleSimplex( int seed, FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
			FN_DECIMAL t = ( x + y + z ) * F3;
			int i = FastFloor( x + t );
			int j = FastFloor( y + t );
			int k = FastFloor( z + t );

			t = ( i + j + k ) * G3;
			FN_DECIMAL x0 = x - ( i - t );
			FN_DECIMAL y0 = y - ( j - t );
			FN_DECIMAL z0 = z - ( k - t );

			int i1, j1, k1;
			int i2, j2, k2;

			if( x0 >= y0 ) {
				if( y0 >= z0 ) {
					i1 = 1;
					j1 = 0;
					k1 = 0;
					i2 = 1;
					j2 = 1;
					k2 = 0;
				} else if( x0 >= z0 ) {
					i1 = 1;
					j1 = 0;
					k1 = 0;
					i2 = 1;
					j2 = 0;
					k2 = 1;
				} else // x0 < z0
				  {
					i1 = 0;
					j1 = 0;
					k1 = 1;
					i2 = 1;
					j2 = 0;
					k2 = 1;
				}
			} else // x0 < y0
			  {
				if( y0 < z0 ) {
					i1 = 0;
					j1 = 0;
					k1 = 1;
					i2 = 0;
					j2 = 1;
					k2 = 1;
				} else if( x0 < z0 ) {
					i1 = 0;
					j1 = 1;
					k1 = 0;
					i2 = 0;
					j2 = 1;
					k2 = 1;
				} else // x0 >= z0
				  {
					i1 = 0;
					j1 = 1;
					k1 = 0;
					i2 = 1;
					j2 = 1;
					k2 = 0;
				}
			}

			FN_DECIMAL x1 = x0 - i1 + G3;
			FN_DECIMAL y1 = y0 - j1 + G3;
			FN_DECIMAL z1 = z0 - k1 + G3;
			FN_DECIMAL x2 = x0 - i2 + F3;
			FN_DECIMAL y2 = y0 - j2 + F3;
			FN_DECIMAL z2 = z0 - k2 + F3;
			FN_DECIMAL x3 = x0 + G33;
			FN_DECIMAL y3 = y0 + G33;
			FN_DECIMAL z3 = z0 + G33;

			FN_DECIMAL n0, n1, n2, n3;

			t = (FN_DECIMAL)0.6 - x0 * x0 - y0 * y0 - z0 * z0;
			if( t < 0 )
				n0 = 0;
			else {
				t *= t;
				n0 = t * t * GradCoord3D( seed, i, j, k, x0, y0, z0 );
			}

			t = (FN_DECIMAL)0.6 - x1 * x1 - y1 * y1 - z1 * z1;
			if( t < 0 )
				n1 = 0;
			else {
				t *= t;
				n1 = t * t * GradCoord3D( seed, i + i1, j + j1, k + k1, x1, y1, z1 );
			}

			t = (FN_DECIMAL)0.6 - x2 * x2 - y2 * y2 - z2 * z2;
			if( t < 0 )
				n2 = 0;
			else {
				t *= t;
				n2 = t * t * GradCoord3D( seed, i + i2, j + j2, k + k2, x2, y2, z2 );
			}

			t = (FN_DECIMAL)0.6 - x3 * x3 - y3 * y3 - z3 * z3;
			if( t < 0 )
				n3 = 0;
			else {
				t *= t;
				n3 = t * t * GradCoord3D( seed, i + 1, j + 1, k + 1, x3, y3, z3 );
			}

			return 32 * ( n0 + n1 + n2 + n3 );
		}

		public FN_DECIMAL GetSimplexFractal( FN_DECIMAL x, FN_DECIMAL y ) {
			x *= m_frequency;
			y *= m_frequency;

			switch( m_fractalType ) {
			case FractalType.FBM:
				return SingleSimplexFractalFBM( x, y );
			case FractalType.Billow:
				return SingleSimplexFractalBillow( x, y );
			case FractalType.RigidMulti:
				return SingleSimplexFractalRigidMulti( x, y );
			default:
				return 0;
			}
		}

		private FN_DECIMAL SingleSimplexFractalFBM( FN_DECIMAL x, FN_DECIMAL y ) {
			int seed = m_seed;
			FN_DECIMAL sum = SingleSimplex( seed, x, y );
			FN_DECIMAL amp = 1;

			for( int i = 1; i < m_octaves; i++ ) {
				x *= m_lacunarity;
				y *= m_lacunarity;

				amp *= m_gain;
				sum += SingleSimplex( ++seed, x, y ) * amp;
			}

			return sum * m_fractalBounding;
		}

		private FN_DECIMAL SingleSimplexFractalBillow( FN_DECIMAL x, FN_DECIMAL y ) {
			int seed = m_seed;
			FN_DECIMAL sum = Math.Abs( SingleSimplex( seed, x, y ) ) * 2 - 1;
			FN_DECIMAL amp = 1;

			for( int i = 1; i < m_octaves; i++ ) {
				x *= m_lacunarity;
				y *= m_lacunarity;

				amp *= m_gain;
				sum += ( Math.Abs( SingleSimplex( ++seed, x, y ) ) * 2 - 1 ) * amp;
			}

			return sum * m_fractalBounding;
		}

		private FN_DECIMAL SingleSimplexFractalRigidMulti( FN_DECIMAL x, FN_DECIMAL y ) {
			int seed = m_seed;
			FN_DECIMAL sum = 1 - Math.Abs( SingleSimplex( seed, x, y ) );
			FN_DECIMAL amp = 1;

			for( int i = 1; i < m_octaves; i++ ) {
				x *= m_lacunarity;
				y *= m_lacunarity;

				amp *= m_gain;
				sum -= ( 1 - Math.Abs( SingleSimplex( ++seed, x, y ) ) ) * amp;
			}

			return sum;
		}

		public FN_DECIMAL GetSimplex( FN_DECIMAL x, FN_DECIMAL y ) {
			return SingleSimplex( m_seed, x * m_frequency, y * m_frequency );
		}

		//private const FN_DECIMAL F2 = (FN_DECIMAL)(1.0 / 2.0);
		//private const FN_DECIMAL G2 = (FN_DECIMAL)(1.0 / 4.0);

		private const FN_DECIMAL SQRT3 = (FN_DECIMAL)1.7320508075688772935274463415059;
		private const FN_DECIMAL F2 = (FN_DECIMAL)0.5 * ( SQRT3 - (FN_DECIMAL)1.0 );
		private const FN_DECIMAL G2 = ( (FN_DECIMAL)3.0 - SQRT3 ) / (FN_DECIMAL)6.0;

		private FN_DECIMAL SingleSimplex( int seed, FN_DECIMAL x, FN_DECIMAL y ) {
			FN_DECIMAL t = ( x + y ) * F2;
			int i = FastFloor( x + t );
			int j = FastFloor( y + t );

			t = ( i + j ) * G2;
			FN_DECIMAL X0 = i - t;
			FN_DECIMAL Y0 = j - t;

			FN_DECIMAL x0 = x - X0;
			FN_DECIMAL y0 = y - Y0;

			int i1, j1;
			if( x0 > y0 ) {
				i1 = 1;
				j1 = 0;
			} else {
				i1 = 0;
				j1 = 1;
			}

			FN_DECIMAL x1 = x0 - i1 + G2;
			FN_DECIMAL y1 = y0 - j1 + G2;
			FN_DECIMAL x2 = x0 - 1 + 2 * G2;
			FN_DECIMAL y2 = y0 - 1 + 2 * G2;

			FN_DECIMAL n0, n1, n2;

			t = (FN_DECIMAL)0.5 - x0 * x0 - y0 * y0;
			if( t < 0 )
				n0 = 0;
			else {
				t *= t;
				n0 = t * t * GradCoord2D( seed, i, j, x0, y0 );
			}

			t = (FN_DECIMAL)0.5 - x1 * x1 - y1 * y1;
			if( t < 0 )
				n1 = 0;
			else {
				t *= t;
				n1 = t * t * GradCoord2D( seed, i + i1, j + j1, x1, y1 );
			}

			t = (FN_DECIMAL)0.5 - x2 * x2 - y2 * y2;
			if( t < 0 )
				n2 = 0;
			else {
				t *= t;
				n2 = t * t * GradCoord2D( seed, i + 1, j + 1, x2, y2 );
			}

			return 50 * ( n0 + n1 + n2 );
		}

		public FN_DECIMAL GetSimplex( FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z, FN_DECIMAL w ) {
			return SingleSimplex( m_seed, x * m_frequency, y * m_frequency, z * m_frequency, w * m_frequency );
		}

		private static readonly byte[] SIMPLEX_4D = {
			0,1,2,3,0,1,3,2,0,0,0,0,0,2,3,1,0,0,0,0,0,0,0,0,0,0,0,0,1,2,3,0,
			0,2,1,3,0,0,0,0,0,3,1,2,0,3,2,1,0,0,0,0,0,0,0,0,0,0,0,0,1,3,2,0,
			0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
			1,2,0,3,0,0,0,0,1,3,0,2,0,0,0,0,0,0,0,0,0,0,0,0,2,3,0,1,2,3,1,0,
			1,0,2,3,1,0,3,2,0,0,0,0,0,0,0,0,0,0,0,0,2,0,3,1,0,0,0,0,2,1,3,0,
			0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
			2,0,1,3,0,0,0,0,0,0,0,0,0,0,0,0,3,0,1,2,3,0,2,1,0,0,0,0,3,1,2,0,
			2,1,0,3,0,0,0,0,0,0,0,0,0,0,0,0,3,1,0,2,0,0,0,0,3,2,0,1,3,2,1,0
		};

		private const FN_DECIMAL F4 = (FN_DECIMAL)( ( 2.23606797 - 1.0 ) / 4.0 );
		private const FN_DECIMAL G4 = (FN_DECIMAL)( ( 5.0 - 2.23606797 ) / 20.0 );

		private FN_DECIMAL SingleSimplex( int seed, FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z, FN_DECIMAL w ) {
			FN_DECIMAL n0, n1, n2, n3, n4;
			FN_DECIMAL t = ( x + y + z + w ) * F4;
			int i = FastFloor( x + t );
			int j = FastFloor( y + t );
			int k = FastFloor( z + t );
			int l = FastFloor( w + t );
			t = ( i + j + k + l ) * G4;
			FN_DECIMAL X0 = i - t;
			FN_DECIMAL Y0 = j - t;
			FN_DECIMAL Z0 = k - t;
			FN_DECIMAL W0 = l - t;
			FN_DECIMAL x0 = x - X0;
			FN_DECIMAL y0 = y - Y0;
			FN_DECIMAL z0 = z - Z0;
			FN_DECIMAL w0 = w - W0;

			int c = ( x0 > y0 ) ? 32 : 0;
			c += ( x0 > z0 ) ? 16 : 0;
			c += ( y0 > z0 ) ? 8 : 0;
			c += ( x0 > w0 ) ? 4 : 0;
			c += ( y0 > w0 ) ? 2 : 0;
			c += ( z0 > w0 ) ? 1 : 0;
			c <<= 2;

			int i1 = SIMPLEX_4D[c] >= 3 ? 1 : 0;
			int i2 = SIMPLEX_4D[c] >= 2 ? 1 : 0;
			int i3 = SIMPLEX_4D[c++] >= 1 ? 1 : 0;
			int j1 = SIMPLEX_4D[c] >= 3 ? 1 : 0;
			int j2 = SIMPLEX_4D[c] >= 2 ? 1 : 0;
			int j3 = SIMPLEX_4D[c++] >= 1 ? 1 : 0;
			int k1 = SIMPLEX_4D[c] >= 3 ? 1 : 0;
			int k2 = SIMPLEX_4D[c] >= 2 ? 1 : 0;
			int k3 = SIMPLEX_4D[c++] >= 1 ? 1 : 0;
			int l1 = SIMPLEX_4D[c] >= 3 ? 1 : 0;
			int l2 = SIMPLEX_4D[c] >= 2 ? 1 : 0;
			int l3 = SIMPLEX_4D[c] >= 1 ? 1 : 0;

			FN_DECIMAL x1 = x0 - i1 + G4;
			FN_DECIMAL y1 = y0 - j1 + G4;
			FN_DECIMAL z1 = z0 - k1 + G4;
			FN_DECIMAL w1 = w0 - l1 + G4;
			FN_DECIMAL x2 = x0 - i2 + 2 * G4;
			FN_DECIMAL y2 = y0 - j2 + 2 * G4;
			FN_DECIMAL z2 = z0 - k2 + 2 * G4;
			FN_DECIMAL w2 = w0 - l2 + 2 * G4;
			FN_DECIMAL x3 = x0 - i3 + 3 * G4;
			FN_DECIMAL y3 = y0 - j3 + 3 * G4;
			FN_DECIMAL z3 = z0 - k3 + 3 * G4;
			FN_DECIMAL w3 = w0 - l3 + 3 * G4;
			FN_DECIMAL x4 = x0 - 1 + 4 * G4;
			FN_DECIMAL y4 = y0 - 1 + 4 * G4;
			FN_DECIMAL z4 = z0 - 1 + 4 * G4;
			FN_DECIMAL w4 = w0 - 1 + 4 * G4;

			t = (FN_DECIMAL)0.6 - x0 * x0 - y0 * y0 - z0 * z0 - w0 * w0;
			if( t < 0 )
				n0 = 0;
			else {
				t *= t;
				n0 = t * t * GradCoord4D( seed, i, j, k, l, x0, y0, z0, w0 );
			}
			t = (FN_DECIMAL)0.6 - x1 * x1 - y1 * y1 - z1 * z1 - w1 * w1;
			if( t < 0 )
				n1 = 0;
			else {
				t *= t;
				n1 = t * t * GradCoord4D( seed, i + i1, j + j1, k + k1, l + l1, x1, y1, z1, w1 );
			}
			t = (FN_DECIMAL)0.6 - x2 * x2 - y2 * y2 - z2 * z2 - w2 * w2;
			if( t < 0 )
				n2 = 0;
			else {
				t *= t;
				n2 = t * t * GradCoord4D( seed, i + i2, j + j2, k + k2, l + l2, x2, y2, z2, w2 );
			}
			t = (FN_DECIMAL)0.6 - x3 * x3 - y3 * y3 - z3 * z3 - w3 * w3;
			if( t < 0 )
				n3 = 0;
			else {
				t *= t;
				n3 = t * t * GradCoord4D( seed, i + i3, j + j3, k + k3, l + l3, x3, y3, z3, w3 );
			}
			t = (FN_DECIMAL)0.6 - x4 * x4 - y4 * y4 - z4 * z4 - w4 * w4;
			if( t < 0 )
				n4 = 0;
			else {
				t *= t;
				n4 = t * t * GradCoord4D( seed, i + 1, j + 1, k + 1, l + 1, x4, y4, z4, w4 );
			}

			return 27 * ( n0 + n1 + n2 + n3 + n4 );
		}
	}
}
