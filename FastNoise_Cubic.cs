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
using System.Runtime.CompilerServices;


namespace FastNoise {
	public partial class FastNoise {
		// Cubic Noise
		public FN_DECIMAL GetCubicFractal( FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
			x *= m_frequency;
			y *= m_frequency;
			z *= m_frequency;

			switch( m_fractalType ) {
			case FractalType.FBM:
				return SingleCubicFractalFBM( x, y, z );
			case FractalType.Billow:
				return SingleCubicFractalBillow( x, y, z );
			case FractalType.RigidMulti:
				return SingleCubicFractalRigidMulti( x, y, z );
			default:
				return 0;
			}
		}

		private FN_DECIMAL SingleCubicFractalFBM( FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
			int seed = m_seed;
			FN_DECIMAL sum = SingleCubic( seed, x, y, z );
			FN_DECIMAL amp = 1;
			int i = 0;

			while( ++i < m_octaves ) {
				x *= m_lacunarity;
				y *= m_lacunarity;
				z *= m_lacunarity;

				amp *= m_gain;
				sum += SingleCubic( ++seed, x, y, z ) * amp;
			}

			return sum * m_fractalBounding;
		}

		private FN_DECIMAL SingleCubicFractalBillow( FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
			int seed = m_seed;
			FN_DECIMAL sum = Math.Abs( SingleCubic( seed, x, y, z ) ) * 2 - 1;
			FN_DECIMAL amp = 1;
			int i = 0;

			while( ++i < m_octaves ) {
				x *= m_lacunarity;
				y *= m_lacunarity;
				z *= m_lacunarity;

				amp *= m_gain;
				sum += ( Math.Abs( SingleCubic( ++seed, x, y, z ) ) * 2 - 1 ) * amp;
			}

			return sum * m_fractalBounding;
		}

		private FN_DECIMAL SingleCubicFractalRigidMulti( FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
			int seed = m_seed;
			FN_DECIMAL sum = 1 - Math.Abs( SingleCubic( seed, x, y, z ) );
			FN_DECIMAL amp = 1;
			int i = 0;

			while( ++i < m_octaves ) {
				x *= m_lacunarity;
				y *= m_lacunarity;
				z *= m_lacunarity;

				amp *= m_gain;
				sum -= ( 1 - Math.Abs( SingleCubic( ++seed, x, y, z ) ) ) * amp;
			}

			return sum;
		}

		public FN_DECIMAL GetCubic( FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
			return SingleCubic( m_seed, x * m_frequency, y * m_frequency, z * m_frequency );
		}

		private const FN_DECIMAL CUBIC_3D_BOUNDING = 1 / (FN_DECIMAL)( 1.5 * 1.5 * 1.5 );

		private FN_DECIMAL SingleCubic( int seed, FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
			int x1 = FastFloor( x );
			int y1 = FastFloor( y );
			int z1 = FastFloor( z );

			int x0 = x1 - 1;
			int y0 = y1 - 1;
			int z0 = z1 - 1;
			int x2 = x1 + 1;
			int y2 = y1 + 1;
			int z2 = z1 + 1;
			int x3 = x1 + 2;
			int y3 = y1 + 2;
			int z3 = z1 + 2;

			FN_DECIMAL xs = x - (FN_DECIMAL)x1;
			FN_DECIMAL ys = y - (FN_DECIMAL)y1;
			FN_DECIMAL zs = z - (FN_DECIMAL)z1;

			return CubicLerp(
				CubicLerp(
				CubicLerp( ValCoord3D( seed, x0, y0, z0 ), ValCoord3D( seed, x1, y0, z0 ), ValCoord3D( seed, x2, y0, z0 ), ValCoord3D( seed, x3, y0, z0 ), xs ),
				CubicLerp( ValCoord3D( seed, x0, y1, z0 ), ValCoord3D( seed, x1, y1, z0 ), ValCoord3D( seed, x2, y1, z0 ), ValCoord3D( seed, x3, y1, z0 ), xs ),
				CubicLerp( ValCoord3D( seed, x0, y2, z0 ), ValCoord3D( seed, x1, y2, z0 ), ValCoord3D( seed, x2, y2, z0 ), ValCoord3D( seed, x3, y2, z0 ), xs ),
				CubicLerp( ValCoord3D( seed, x0, y3, z0 ), ValCoord3D( seed, x1, y3, z0 ), ValCoord3D( seed, x2, y3, z0 ), ValCoord3D( seed, x3, y3, z0 ), xs ),
				ys ),
				CubicLerp(
				CubicLerp( ValCoord3D( seed, x0, y0, z1 ), ValCoord3D( seed, x1, y0, z1 ), ValCoord3D( seed, x2, y0, z1 ), ValCoord3D( seed, x3, y0, z1 ), xs ),
				CubicLerp( ValCoord3D( seed, x0, y1, z1 ), ValCoord3D( seed, x1, y1, z1 ), ValCoord3D( seed, x2, y1, z1 ), ValCoord3D( seed, x3, y1, z1 ), xs ),
				CubicLerp( ValCoord3D( seed, x0, y2, z1 ), ValCoord3D( seed, x1, y2, z1 ), ValCoord3D( seed, x2, y2, z1 ), ValCoord3D( seed, x3, y2, z1 ), xs ),
				CubicLerp( ValCoord3D( seed, x0, y3, z1 ), ValCoord3D( seed, x1, y3, z1 ), ValCoord3D( seed, x2, y3, z1 ), ValCoord3D( seed, x3, y3, z1 ), xs ),
				ys ),
				CubicLerp(
				CubicLerp( ValCoord3D( seed, x0, y0, z2 ), ValCoord3D( seed, x1, y0, z2 ), ValCoord3D( seed, x2, y0, z2 ), ValCoord3D( seed, x3, y0, z2 ), xs ),
				CubicLerp( ValCoord3D( seed, x0, y1, z2 ), ValCoord3D( seed, x1, y1, z2 ), ValCoord3D( seed, x2, y1, z2 ), ValCoord3D( seed, x3, y1, z2 ), xs ),
				CubicLerp( ValCoord3D( seed, x0, y2, z2 ), ValCoord3D( seed, x1, y2, z2 ), ValCoord3D( seed, x2, y2, z2 ), ValCoord3D( seed, x3, y2, z2 ), xs ),
				CubicLerp( ValCoord3D( seed, x0, y3, z2 ), ValCoord3D( seed, x1, y3, z2 ), ValCoord3D( seed, x2, y3, z2 ), ValCoord3D( seed, x3, y3, z2 ), xs ),
				ys ),
				CubicLerp(
				CubicLerp( ValCoord3D( seed, x0, y0, z3 ), ValCoord3D( seed, x1, y0, z3 ), ValCoord3D( seed, x2, y0, z3 ), ValCoord3D( seed, x3, y0, z3 ), xs ),
				CubicLerp( ValCoord3D( seed, x0, y1, z3 ), ValCoord3D( seed, x1, y1, z3 ), ValCoord3D( seed, x2, y1, z3 ), ValCoord3D( seed, x3, y1, z3 ), xs ),
				CubicLerp( ValCoord3D( seed, x0, y2, z3 ), ValCoord3D( seed, x1, y2, z3 ), ValCoord3D( seed, x2, y2, z3 ), ValCoord3D( seed, x3, y2, z3 ), xs ),
				CubicLerp( ValCoord3D( seed, x0, y3, z3 ), ValCoord3D( seed, x1, y3, z3 ), ValCoord3D( seed, x2, y3, z3 ), ValCoord3D( seed, x3, y3, z3 ), xs ),
				ys ),
				zs ) * CUBIC_3D_BOUNDING;
		}


		public FN_DECIMAL GetCubicFractal( FN_DECIMAL x, FN_DECIMAL y ) {
			x *= m_frequency;
			y *= m_frequency;

			switch( m_fractalType ) {
			case FractalType.FBM:
				return SingleCubicFractalFBM( x, y );
			case FractalType.Billow:
				return SingleCubicFractalBillow( x, y );
			case FractalType.RigidMulti:
				return SingleCubicFractalRigidMulti( x, y );
			default:
				return 0;
			}
		}

		private FN_DECIMAL SingleCubicFractalFBM( FN_DECIMAL x, FN_DECIMAL y ) {
			int seed = m_seed;
			FN_DECIMAL sum = SingleCubic( seed, x, y );
			FN_DECIMAL amp = 1;
			int i = 0;

			while( ++i < m_octaves ) {
				x *= m_lacunarity;
				y *= m_lacunarity;

				amp *= m_gain;
				sum += SingleCubic( ++seed, x, y ) * amp;
			}

			return sum * m_fractalBounding;
		}

		private FN_DECIMAL SingleCubicFractalBillow( FN_DECIMAL x, FN_DECIMAL y ) {
			int seed = m_seed;
			FN_DECIMAL sum = Math.Abs( SingleCubic( seed, x, y ) ) * 2 - 1;
			FN_DECIMAL amp = 1;
			int i = 0;

			while( ++i < m_octaves ) {
				x *= m_lacunarity;
				y *= m_lacunarity;

				amp *= m_gain;
				sum += ( Math.Abs( SingleCubic( ++seed, x, y ) ) * 2 - 1 ) * amp;
			}

			return sum * m_fractalBounding;
		}

		private FN_DECIMAL SingleCubicFractalRigidMulti( FN_DECIMAL x, FN_DECIMAL y ) {
			int seed = m_seed;
			FN_DECIMAL sum = 1 - Math.Abs( SingleCubic( seed, x, y ) );
			FN_DECIMAL amp = 1;
			int i = 0;

			while( ++i < m_octaves ) {
				x *= m_lacunarity;
				y *= m_lacunarity;

				amp *= m_gain;
				sum -= ( 1 - Math.Abs( SingleCubic( ++seed, x, y ) ) ) * amp;
			}

			return sum;
		}

		public FN_DECIMAL GetCubic( FN_DECIMAL x, FN_DECIMAL y ) {
			x *= m_frequency;
			y *= m_frequency;

			return SingleCubic( 0, x, y );
		}

		private const FN_DECIMAL CUBIC_2D_BOUNDING = 1 / (FN_DECIMAL)( 1.5 * 1.5 );

		private FN_DECIMAL SingleCubic( int seed, FN_DECIMAL x, FN_DECIMAL y ) {
			int x1 = FastFloor( x );
			int y1 = FastFloor( y );

			int x0 = x1 - 1;
			int y0 = y1 - 1;
			int x2 = x1 + 1;
			int y2 = y1 + 1;
			int x3 = x1 + 2;
			int y3 = y1 + 2;

			FN_DECIMAL xs = x - (FN_DECIMAL)x1;
			FN_DECIMAL ys = y - (FN_DECIMAL)y1;

			return CubicLerp(
					   CubicLerp( ValCoord2D( seed, x0, y0 ), ValCoord2D( seed, x1, y0 ), ValCoord2D( seed, x2, y0 ), ValCoord2D( seed, x3, y0 ),
						   xs ),
					   CubicLerp( ValCoord2D( seed, x0, y1 ), ValCoord2D( seed, x1, y1 ), ValCoord2D( seed, x2, y1 ), ValCoord2D( seed, x3, y1 ),
						   xs ),
					   CubicLerp( ValCoord2D( seed, x0, y2 ), ValCoord2D( seed, x1, y2 ), ValCoord2D( seed, x2, y2 ), ValCoord2D( seed, x3, y2 ),
						   xs ),
					   CubicLerp( ValCoord2D( seed, x0, y3 ), ValCoord2D( seed, x1, y3 ), ValCoord2D( seed, x2, y3 ), ValCoord2D( seed, x3, y3 ),
						   xs ),
					   ys ) * CUBIC_2D_BOUNDING;
		}
	}
}
