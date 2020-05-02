﻿// FastNoise.cs
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
		// Gradient Noise
		public FN_DECIMAL GetPerlinFractal( FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
			x *= m_frequency;
			y *= m_frequency;
			z *= m_frequency;

			switch( m_fractalType ) {
			case FractalType.FBM:
				return SinglePerlinFractalFBM( x, y, z );
			case FractalType.Billow:
				return SinglePerlinFractalBillow( x, y, z );
			case FractalType.RigidMulti:
				return SinglePerlinFractalRigidMulti( x, y, z );
			default:
				return 0;
			}
		}

		private FN_DECIMAL SinglePerlinFractalFBM( FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
			int seed = m_seed;
			FN_DECIMAL sum = SinglePerlin( seed, x, y, z );
			FN_DECIMAL amp = 1;

			for( int i = 1; i < m_octaves; i++ ) {
				x *= m_lacunarity;
				y *= m_lacunarity;
				z *= m_lacunarity;

				amp *= m_gain;
				sum += SinglePerlin( ++seed, x, y, z ) * amp;
			}

			return sum * m_fractalBounding;
		}

		private FN_DECIMAL SinglePerlinFractalBillow( FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
			int seed = m_seed;
			FN_DECIMAL sum = Math.Abs( SinglePerlin( seed, x, y, z ) ) * 2 - 1;
			FN_DECIMAL amp = 1;

			for( int i = 1; i < m_octaves; i++ ) {
				x *= m_lacunarity;
				y *= m_lacunarity;
				z *= m_lacunarity;

				amp *= m_gain;
				sum += ( Math.Abs( SinglePerlin( ++seed, x, y, z ) ) * 2 - 1 ) * amp;
			}

			return sum * m_fractalBounding;
		}

		private FN_DECIMAL SinglePerlinFractalRigidMulti( FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
			int seed = m_seed;
			FN_DECIMAL sum = 1 - Math.Abs( SinglePerlin( seed, x, y, z ) );
			FN_DECIMAL amp = 1;

			for( int i = 1; i < m_octaves; i++ ) {
				x *= m_lacunarity;
				y *= m_lacunarity;
				z *= m_lacunarity;

				amp *= m_gain;
				sum -= ( 1 - Math.Abs( SinglePerlin( ++seed, x, y, z ) ) ) * amp;
			}

			return sum;
		}

		public FN_DECIMAL GetPerlin( FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
			return SinglePerlin( m_seed, x * m_frequency, y * m_frequency, z * m_frequency );
		}

		private FN_DECIMAL SinglePerlin( int seed, FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
			int x0 = FastFloor( x );
			int y0 = FastFloor( y );
			int z0 = FastFloor( z );
			int x1 = x0 + 1;
			int y1 = y0 + 1;
			int z1 = z0 + 1;

			FN_DECIMAL xs, ys, zs;
			switch( m_interp ) {
			default:
			case Interp.Linear:
				xs = x - x0;
				ys = y - y0;
				zs = z - z0;
				break;
			case Interp.Hermite:
				xs = InterpHermiteFunc( x - x0 );
				ys = InterpHermiteFunc( y - y0 );
				zs = InterpHermiteFunc( z - z0 );
				break;
			case Interp.Quintic:
				xs = InterpQuinticFunc( x - x0 );
				ys = InterpQuinticFunc( y - y0 );
				zs = InterpQuinticFunc( z - z0 );
				break;
			}

			FN_DECIMAL xd0 = x - x0;
			FN_DECIMAL yd0 = y - y0;
			FN_DECIMAL zd0 = z - z0;
			FN_DECIMAL xd1 = xd0 - 1;
			FN_DECIMAL yd1 = yd0 - 1;
			FN_DECIMAL zd1 = zd0 - 1;

			FN_DECIMAL xf00 = Lerp( GradCoord3D( seed, x0, y0, z0, xd0, yd0, zd0 ), GradCoord3D( seed, x1, y0, z0, xd1, yd0, zd0 ), xs );
			FN_DECIMAL xf10 = Lerp( GradCoord3D( seed, x0, y1, z0, xd0, yd1, zd0 ), GradCoord3D( seed, x1, y1, z0, xd1, yd1, zd0 ), xs );
			FN_DECIMAL xf01 = Lerp( GradCoord3D( seed, x0, y0, z1, xd0, yd0, zd1 ), GradCoord3D( seed, x1, y0, z1, xd1, yd0, zd1 ), xs );
			FN_DECIMAL xf11 = Lerp( GradCoord3D( seed, x0, y1, z1, xd0, yd1, zd1 ), GradCoord3D( seed, x1, y1, z1, xd1, yd1, zd1 ), xs );

			FN_DECIMAL yf0 = Lerp( xf00, xf10, ys );
			FN_DECIMAL yf1 = Lerp( xf01, xf11, ys );

			return Lerp( yf0, yf1, zs );
		}

		public FN_DECIMAL GetPerlinFractal( FN_DECIMAL x, FN_DECIMAL y ) {
			x *= m_frequency;
			y *= m_frequency;

			switch( m_fractalType ) {
			case FractalType.FBM:
				return SinglePerlinFractalFBM( x, y );
			case FractalType.Billow:
				return SinglePerlinFractalBillow( x, y );
			case FractalType.RigidMulti:
				return SinglePerlinFractalRigidMulti( x, y );
			default:
				return 0;
			}
		}

		private FN_DECIMAL SinglePerlinFractalFBM( FN_DECIMAL x, FN_DECIMAL y ) {
			int seed = m_seed;
			FN_DECIMAL sum = SinglePerlin( seed, x, y );
			FN_DECIMAL amp = 1;

			for( int i = 1; i < m_octaves; i++ ) {
				x *= m_lacunarity;
				y *= m_lacunarity;

				amp *= m_gain;
				sum += SinglePerlin( ++seed, x, y ) * amp;
			}

			return sum * m_fractalBounding;
		}

		private FN_DECIMAL SinglePerlinFractalBillow( FN_DECIMAL x, FN_DECIMAL y ) {
			int seed = m_seed;
			FN_DECIMAL sum = Math.Abs( SinglePerlin( seed, x, y ) ) * 2 - 1;
			FN_DECIMAL amp = 1;

			for( int i = 1; i < m_octaves; i++ ) {
				x *= m_lacunarity;
				y *= m_lacunarity;

				amp *= m_gain;
				sum += ( Math.Abs( SinglePerlin( ++seed, x, y ) ) * 2 - 1 ) * amp;
			}

			return sum * m_fractalBounding;
		}

		private FN_DECIMAL SinglePerlinFractalRigidMulti( FN_DECIMAL x, FN_DECIMAL y ) {
			int seed = m_seed;
			FN_DECIMAL sum = 1 - Math.Abs( SinglePerlin( seed, x, y ) );
			FN_DECIMAL amp = 1;

			for( int i = 1; i < m_octaves; i++ ) {
				x *= m_lacunarity;
				y *= m_lacunarity;

				amp *= m_gain;
				sum -= ( 1 - Math.Abs( SinglePerlin( ++seed, x, y ) ) ) * amp;
			}

			return sum;
		}

		public FN_DECIMAL GetPerlin( FN_DECIMAL x, FN_DECIMAL y ) {
			return SinglePerlin( m_seed, x * m_frequency, y * m_frequency );
		}

		private FN_DECIMAL SinglePerlin( int seed, FN_DECIMAL x, FN_DECIMAL y ) {
			int x0 = FastFloor( x );
			int y0 = FastFloor( y );
			int x1 = x0 + 1;
			int y1 = y0 + 1;

			FN_DECIMAL xs, ys;
			switch( m_interp ) {
			default:
			case Interp.Linear:
				xs = x - x0;
				ys = y - y0;
				break;
			case Interp.Hermite:
				xs = InterpHermiteFunc( x - x0 );
				ys = InterpHermiteFunc( y - y0 );
				break;
			case Interp.Quintic:
				xs = InterpQuinticFunc( x - x0 );
				ys = InterpQuinticFunc( y - y0 );
				break;
			}

			FN_DECIMAL xd0 = x - x0;
			FN_DECIMAL yd0 = y - y0;
			FN_DECIMAL xd1 = xd0 - 1;
			FN_DECIMAL yd1 = yd0 - 1;

			FN_DECIMAL xf0 = Lerp( GradCoord2D( seed, x0, y0, xd0, yd0 ), GradCoord2D( seed, x1, y0, xd1, yd0 ), xs );
			FN_DECIMAL xf1 = Lerp( GradCoord2D( seed, x0, y1, xd0, yd1 ), GradCoord2D( seed, x1, y1, xd1, yd1 ), xs );

			return Lerp( xf0, xf1, ys );
		}
	}
}
