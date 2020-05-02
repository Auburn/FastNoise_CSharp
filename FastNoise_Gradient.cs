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
		public void GradientPerturb( ref FN_DECIMAL x, ref FN_DECIMAL y, ref FN_DECIMAL z ) {
			SingleGradientPerturb( m_seed, m_gradientPerturbAmp, m_frequency, ref x, ref y, ref z );
		}

		public void GradientPerturbFractal( ref FN_DECIMAL x, ref FN_DECIMAL y, ref FN_DECIMAL z ) {
			int seed = m_seed;
			FN_DECIMAL amp = m_gradientPerturbAmp * m_fractalBounding;
			FN_DECIMAL freq = m_frequency;

			SingleGradientPerturb( seed, amp, m_frequency, ref x, ref y, ref z );

			for( int i = 1; i < m_octaves; i++ ) {
				freq *= m_lacunarity;
				amp *= m_gain;
				SingleGradientPerturb( ++seed, amp, freq, ref x, ref y, ref z );
			}
		}

		private void SingleGradientPerturb( int seed, FN_DECIMAL perturbAmp, FN_DECIMAL frequency, ref FN_DECIMAL x, ref FN_DECIMAL y, ref FN_DECIMAL z ) {
			FN_DECIMAL xf = x * frequency;
			FN_DECIMAL yf = y * frequency;
			FN_DECIMAL zf = z * frequency;

			int x0 = FastFloor( xf );
			int y0 = FastFloor( yf );
			int z0 = FastFloor( zf );
			int x1 = x0 + 1;
			int y1 = y0 + 1;
			int z1 = z0 + 1;

			FN_DECIMAL xs, ys, zs;
			switch( m_interp ) {
			default:
			case Interp.Linear:
				xs = xf - x0;
				ys = yf - y0;
				zs = zf - z0;
				break;
			case Interp.Hermite:
				xs = InterpHermiteFunc( xf - x0 );
				ys = InterpHermiteFunc( yf - y0 );
				zs = InterpHermiteFunc( zf - z0 );
				break;
			case Interp.Quintic:
				xs = InterpQuinticFunc( xf - x0 );
				ys = InterpQuinticFunc( yf - y0 );
				zs = InterpQuinticFunc( zf - z0 );
				break;
			}

			Float3 vec0 = CELL_3D[Hash3D( seed, x0, y0, z0 ) & 255];
			Float3 vec1 = CELL_3D[Hash3D( seed, x1, y0, z0 ) & 255];

			FN_DECIMAL lx0x = Lerp( vec0.x, vec1.x, xs );
			FN_DECIMAL ly0x = Lerp( vec0.y, vec1.y, xs );
			FN_DECIMAL lz0x = Lerp( vec0.z, vec1.z, xs );

			vec0 = CELL_3D[Hash3D( seed, x0, y1, z0 ) & 255];
			vec1 = CELL_3D[Hash3D( seed, x1, y1, z0 ) & 255];

			FN_DECIMAL lx1x = Lerp( vec0.x, vec1.x, xs );
			FN_DECIMAL ly1x = Lerp( vec0.y, vec1.y, xs );
			FN_DECIMAL lz1x = Lerp( vec0.z, vec1.z, xs );

			FN_DECIMAL lx0y = Lerp( lx0x, lx1x, ys );
			FN_DECIMAL ly0y = Lerp( ly0x, ly1x, ys );
			FN_DECIMAL lz0y = Lerp( lz0x, lz1x, ys );

			vec0 = CELL_3D[Hash3D( seed, x0, y0, z1 ) & 255];
			vec1 = CELL_3D[Hash3D( seed, x1, y0, z1 ) & 255];

			lx0x = Lerp( vec0.x, vec1.x, xs );
			ly0x = Lerp( vec0.y, vec1.y, xs );
			lz0x = Lerp( vec0.z, vec1.z, xs );

			vec0 = CELL_3D[Hash3D( seed, x0, y1, z1 ) & 255];
			vec1 = CELL_3D[Hash3D( seed, x1, y1, z1 ) & 255];

			lx1x = Lerp( vec0.x, vec1.x, xs );
			ly1x = Lerp( vec0.y, vec1.y, xs );
			lz1x = Lerp( vec0.z, vec1.z, xs );

			x += Lerp( lx0y, Lerp( lx0x, lx1x, ys ), zs ) * perturbAmp;
			y += Lerp( ly0y, Lerp( ly0x, ly1x, ys ), zs ) * perturbAmp;
			z += Lerp( lz0y, Lerp( lz0x, lz1x, ys ), zs ) * perturbAmp;
		}

		public void GradientPerturb( ref FN_DECIMAL x, ref FN_DECIMAL y ) {
			SingleGradientPerturb( m_seed, m_gradientPerturbAmp, m_frequency, ref x, ref y );
		}

		public void GradientPerturbFractal( ref FN_DECIMAL x, ref FN_DECIMAL y ) {
			int seed = m_seed;
			FN_DECIMAL amp = m_gradientPerturbAmp * m_fractalBounding;
			FN_DECIMAL freq = m_frequency;

			SingleGradientPerturb( seed, amp, m_frequency, ref x, ref y );

			for( int i = 1; i < m_octaves; i++ ) {
				freq *= m_lacunarity;
				amp *= m_gain;
				SingleGradientPerturb( ++seed, amp, freq, ref x, ref y );
			}
		}

		private void SingleGradientPerturb( int seed, FN_DECIMAL perturbAmp, FN_DECIMAL frequency, ref FN_DECIMAL x, ref FN_DECIMAL y ) {
			FN_DECIMAL xf = x * frequency;
			FN_DECIMAL yf = y * frequency;

			int x0 = FastFloor( xf );
			int y0 = FastFloor( yf );
			int x1 = x0 + 1;
			int y1 = y0 + 1;

			FN_DECIMAL xs, ys;
			switch( m_interp ) {
			default:
			case Interp.Linear:
				xs = xf - x0;
				ys = yf - y0;
				break;
			case Interp.Hermite:
				xs = InterpHermiteFunc( xf - x0 );
				ys = InterpHermiteFunc( yf - y0 );
				break;
			case Interp.Quintic:
				xs = InterpQuinticFunc( xf - x0 );
				ys = InterpQuinticFunc( yf - y0 );
				break;
			}

			Float2 vec0 = CELL_2D[Hash2D( seed, x0, y0 ) & 255];
			Float2 vec1 = CELL_2D[Hash2D( seed, x1, y0 ) & 255];

			FN_DECIMAL lx0x = Lerp( vec0.x, vec1.x, xs );
			FN_DECIMAL ly0x = Lerp( vec0.y, vec1.y, xs );

			vec0 = CELL_2D[Hash2D( seed, x0, y1 ) & 255];
			vec1 = CELL_2D[Hash2D( seed, x1, y1 ) & 255];

			FN_DECIMAL lx1x = Lerp( vec0.x, vec1.x, xs );
			FN_DECIMAL ly1x = Lerp( vec0.y, vec1.y, xs );

			x += Lerp( lx0x, lx1x, ys ) * perturbAmp;
			y += Lerp( ly0x, ly1x, ys ) * perturbAmp;
		}
	}
}
