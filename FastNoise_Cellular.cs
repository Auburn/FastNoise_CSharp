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


public partial class FastNoise {
	// Cellular Noise
	public FN_DECIMAL GetCellular( FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
		x *= m_frequency;
		y *= m_frequency;
		z *= m_frequency;

		switch( m_cellularReturnType ) {
		case CellularReturnType.CellValue:
		case CellularReturnType.NoiseLookup:
		case CellularReturnType.Distance:
			return SingleCellular( x, y, z );
		default:
			return SingleCellular2Edge( x, y, z );
		}
	}

	private FN_DECIMAL SingleCellular( FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
		int xr = FastRound( x );
		int yr = FastRound( y );
		int zr = FastRound( z );

		FN_DECIMAL distance = 999999;
		int xc = 0, yc = 0, zc = 0;

		switch( m_cellularDistanceFunction ) {
		case CellularDistanceFunction.Euclidean:
			for( int xi = xr - 1; xi <= xr + 1; xi++ ) {
				for( int yi = yr - 1; yi <= yr + 1; yi++ ) {
					for( int zi = zr - 1; zi <= zr + 1; zi++ ) {
						Float3 vec = CELL_3D[Hash3D( m_seed, xi, yi, zi ) & 255];

						FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
						FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;
						FN_DECIMAL vecZ = zi - z + vec.z * m_cellularJitter;

						FN_DECIMAL newDistance = vecX * vecX + vecY * vecY + vecZ * vecZ;

						if( newDistance < distance ) {
							distance = newDistance;
							xc = xi;
							yc = yi;
							zc = zi;
						}
					}
				}
			}
			break;
		case CellularDistanceFunction.Manhattan:
			for( int xi = xr - 1; xi <= xr + 1; xi++ ) {
				for( int yi = yr - 1; yi <= yr + 1; yi++ ) {
					for( int zi = zr - 1; zi <= zr + 1; zi++ ) {
						Float3 vec = CELL_3D[Hash3D( m_seed, xi, yi, zi ) & 255];

						FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
						FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;
						FN_DECIMAL vecZ = zi - z + vec.z * m_cellularJitter;

						FN_DECIMAL newDistance = Math.Abs( vecX ) + Math.Abs( vecY ) + Math.Abs( vecZ );

						if( newDistance < distance ) {
							distance = newDistance;
							xc = xi;
							yc = yi;
							zc = zi;
						}
					}
				}
			}
			break;
		case CellularDistanceFunction.Natural:
			for( int xi = xr - 1; xi <= xr + 1; xi++ ) {
				for( int yi = yr - 1; yi <= yr + 1; yi++ ) {
					for( int zi = zr - 1; zi <= zr + 1; zi++ ) {
						Float3 vec = CELL_3D[Hash3D( m_seed, xi, yi, zi ) & 255];

						FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
						FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;
						FN_DECIMAL vecZ = zi - z + vec.z * m_cellularJitter;

						FN_DECIMAL newDistance = ( Math.Abs( vecX ) + Math.Abs( vecY ) + Math.Abs( vecZ ) ) + ( vecX * vecX + vecY * vecY + vecZ * vecZ );

						if( newDistance < distance ) {
							distance = newDistance;
							xc = xi;
							yc = yi;
							zc = zi;
						}
					}
				}
			}
			break;
		}

		switch( m_cellularReturnType ) {
		case CellularReturnType.CellValue:
			return ValCoord3D( m_seed, xc, yc, zc );

		case CellularReturnType.NoiseLookup:
			Float3 vec = CELL_3D[Hash3D( m_seed, xc, yc, zc ) & 255];
			return m_cellularNoiseLookup.GetNoise( xc + vec.x * m_cellularJitter, yc + vec.y * m_cellularJitter, zc + vec.z * m_cellularJitter );

		case CellularReturnType.Distance:
			return distance;
		default:
			return 0;
		}
	}

	private FN_DECIMAL SingleCellular2Edge( FN_DECIMAL x, FN_DECIMAL y, FN_DECIMAL z ) {
		int xr = FastRound( x );
		int yr = FastRound( y );
		int zr = FastRound( z );

		FN_DECIMAL[] distance = { 999999, 999999, 999999, 999999 };

		switch( m_cellularDistanceFunction ) {
		case CellularDistanceFunction.Euclidean:
			for( int xi = xr - 1; xi <= xr + 1; xi++ ) {
				for( int yi = yr - 1; yi <= yr + 1; yi++ ) {
					for( int zi = zr - 1; zi <= zr + 1; zi++ ) {
						Float3 vec = CELL_3D[Hash3D( m_seed, xi, yi, zi ) & 255];

						FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
						FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;
						FN_DECIMAL vecZ = zi - z + vec.z * m_cellularJitter;

						FN_DECIMAL newDistance = vecX * vecX + vecY * vecY + vecZ * vecZ;

						for( int i = m_cellularDistanceIndex1; i > 0; i-- )
							distance[i] = Math.Max( Math.Min( distance[i], newDistance ), distance[i - 1] );
						distance[0] = Math.Min( distance[0], newDistance );
					}
				}
			}
			break;
		case CellularDistanceFunction.Manhattan:
			for( int xi = xr - 1; xi <= xr + 1; xi++ ) {
				for( int yi = yr - 1; yi <= yr + 1; yi++ ) {
					for( int zi = zr - 1; zi <= zr + 1; zi++ ) {
						Float3 vec = CELL_3D[Hash3D( m_seed, xi, yi, zi ) & 255];

						FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
						FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;
						FN_DECIMAL vecZ = zi - z + vec.z * m_cellularJitter;

						FN_DECIMAL newDistance = Math.Abs( vecX ) + Math.Abs( vecY ) + Math.Abs( vecZ );

						for( int i = m_cellularDistanceIndex1; i > 0; i-- )
							distance[i] = Math.Max( Math.Min( distance[i], newDistance ), distance[i - 1] );
						distance[0] = Math.Min( distance[0], newDistance );
					}
				}
			}
			break;
		case CellularDistanceFunction.Natural:
			for( int xi = xr - 1; xi <= xr + 1; xi++ ) {
				for( int yi = yr - 1; yi <= yr + 1; yi++ ) {
					for( int zi = zr - 1; zi <= zr + 1; zi++ ) {
						Float3 vec = CELL_3D[Hash3D( m_seed, xi, yi, zi ) & 255];

						FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
						FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;
						FN_DECIMAL vecZ = zi - z + vec.z * m_cellularJitter;

						FN_DECIMAL newDistance = ( Math.Abs( vecX ) + Math.Abs( vecY ) + Math.Abs( vecZ ) ) + ( vecX * vecX + vecY * vecY + vecZ * vecZ );

						for( int i = m_cellularDistanceIndex1; i > 0; i-- )
							distance[i] = Math.Max( Math.Min( distance[i], newDistance ), distance[i - 1] );
						distance[0] = Math.Min( distance[0], newDistance );
					}
				}
			}
			break;
		default:
			break;
		}

		switch( m_cellularReturnType ) {
		case CellularReturnType.Distance2:
			return distance[m_cellularDistanceIndex1];
		case CellularReturnType.Distance2Add:
			return distance[m_cellularDistanceIndex1] + distance[m_cellularDistanceIndex0];
		case CellularReturnType.Distance2Sub:
			return distance[m_cellularDistanceIndex1] - distance[m_cellularDistanceIndex0];
		case CellularReturnType.Distance2Mul:
			return distance[m_cellularDistanceIndex1] * distance[m_cellularDistanceIndex0];
		case CellularReturnType.Distance2Div:
			return distance[m_cellularDistanceIndex0] / distance[m_cellularDistanceIndex1];
		default:
			return 0;
		}
	}

	public FN_DECIMAL GetCellular( FN_DECIMAL x, FN_DECIMAL y ) {
		x *= m_frequency;
		y *= m_frequency;

		switch( m_cellularReturnType ) {
		case CellularReturnType.CellValue:
		case CellularReturnType.NoiseLookup:
		case CellularReturnType.Distance:
			return SingleCellular( x, y );
		default:
			return SingleCellular2Edge( x, y );
		}
	}

	private FN_DECIMAL SingleCellular( FN_DECIMAL x, FN_DECIMAL y ) {
		int xr = FastRound( x );
		int yr = FastRound( y );

		FN_DECIMAL distance = 999999;
		int xc = 0, yc = 0;

		switch( m_cellularDistanceFunction ) {
		default:
		case CellularDistanceFunction.Euclidean:
			for( int xi = xr - 1; xi <= xr + 1; xi++ ) {
				for( int yi = yr - 1; yi <= yr + 1; yi++ ) {
					Float2 vec = CELL_2D[Hash2D( m_seed, xi, yi ) & 255];

					FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
					FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;

					FN_DECIMAL newDistance = vecX * vecX + vecY * vecY;

					if( newDistance < distance ) {
						distance = newDistance;
						xc = xi;
						yc = yi;
					}
				}
			}
			break;
		case CellularDistanceFunction.Manhattan:
			for( int xi = xr - 1; xi <= xr + 1; xi++ ) {
				for( int yi = yr - 1; yi <= yr + 1; yi++ ) {
					Float2 vec = CELL_2D[Hash2D( m_seed, xi, yi ) & 255];

					FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
					FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;

					FN_DECIMAL newDistance = ( Math.Abs( vecX ) + Math.Abs( vecY ) );

					if( newDistance < distance ) {
						distance = newDistance;
						xc = xi;
						yc = yi;
					}
				}
			}
			break;
		case CellularDistanceFunction.Natural:
			for( int xi = xr - 1; xi <= xr + 1; xi++ ) {
				for( int yi = yr - 1; yi <= yr + 1; yi++ ) {
					Float2 vec = CELL_2D[Hash2D( m_seed, xi, yi ) & 255];

					FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
					FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;

					FN_DECIMAL newDistance = ( Math.Abs( vecX ) + Math.Abs( vecY ) ) + ( vecX * vecX + vecY * vecY );

					if( newDistance < distance ) {
						distance = newDistance;
						xc = xi;
						yc = yi;
					}
				}
			}
			break;
		}

		switch( m_cellularReturnType ) {
		case CellularReturnType.CellValue:
			return ValCoord2D( m_seed, xc, yc );

		case CellularReturnType.NoiseLookup:
			Float2 vec = CELL_2D[Hash2D( m_seed, xc, yc ) & 255];
			return m_cellularNoiseLookup.GetNoise( xc + vec.x * m_cellularJitter, yc + vec.y * m_cellularJitter );

		case CellularReturnType.Distance:
			return distance;
		default:
			return 0;
		}
	}

	private FN_DECIMAL SingleCellular2Edge( FN_DECIMAL x, FN_DECIMAL y ) {
		int xr = FastRound( x );
		int yr = FastRound( y );

		FN_DECIMAL[] distance = { 999999, 999999, 999999, 999999 };

		switch( m_cellularDistanceFunction ) {
		default:
		case CellularDistanceFunction.Euclidean:
			for( int xi = xr - 1; xi <= xr + 1; xi++ ) {
				for( int yi = yr - 1; yi <= yr + 1; yi++ ) {
					Float2 vec = CELL_2D[Hash2D( m_seed, xi, yi ) & 255];

					FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
					FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;

					FN_DECIMAL newDistance = vecX * vecX + vecY * vecY;

					for( int i = m_cellularDistanceIndex1; i > 0; i-- )
						distance[i] = Math.Max( Math.Min( distance[i], newDistance ), distance[i - 1] );
					distance[0] = Math.Min( distance[0], newDistance );
				}
			}
			break;
		case CellularDistanceFunction.Manhattan:
			for( int xi = xr - 1; xi <= xr + 1; xi++ ) {
				for( int yi = yr - 1; yi <= yr + 1; yi++ ) {
					Float2 vec = CELL_2D[Hash2D( m_seed, xi, yi ) & 255];

					FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
					FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;

					FN_DECIMAL newDistance = Math.Abs( vecX ) + Math.Abs( vecY );

					for( int i = m_cellularDistanceIndex1; i > 0; i-- )
						distance[i] = Math.Max( Math.Min( distance[i], newDistance ), distance[i - 1] );
					distance[0] = Math.Min( distance[0], newDistance );
				}
			}
			break;
		case CellularDistanceFunction.Natural:
			for( int xi = xr - 1; xi <= xr + 1; xi++ ) {
				for( int yi = yr - 1; yi <= yr + 1; yi++ ) {
					Float2 vec = CELL_2D[Hash2D( m_seed, xi, yi ) & 255];

					FN_DECIMAL vecX = xi - x + vec.x * m_cellularJitter;
					FN_DECIMAL vecY = yi - y + vec.y * m_cellularJitter;

					FN_DECIMAL newDistance = ( Math.Abs( vecX ) + Math.Abs( vecY ) ) + ( vecX * vecX + vecY * vecY );

					for( int i = m_cellularDistanceIndex1; i > 0; i-- )
						distance[i] = Math.Max( Math.Min( distance[i], newDistance ), distance[i - 1] );
					distance[0] = Math.Min( distance[0], newDistance );
				}
			}
			break;
		}

		switch( m_cellularReturnType ) {
		case CellularReturnType.Distance2:
			return distance[m_cellularDistanceIndex1];
		case CellularReturnType.Distance2Add:
			return distance[m_cellularDistanceIndex1] + distance[m_cellularDistanceIndex0];
		case CellularReturnType.Distance2Sub:
			return distance[m_cellularDistanceIndex1] - distance[m_cellularDistanceIndex0];
		case CellularReturnType.Distance2Mul:
			return distance[m_cellularDistanceIndex1] * distance[m_cellularDistanceIndex0];
		case CellularReturnType.Distance2Div:
			return distance[m_cellularDistanceIndex0] / distance[m_cellularDistanceIndex1];
		default:
			return 0;
		}
	}
}
