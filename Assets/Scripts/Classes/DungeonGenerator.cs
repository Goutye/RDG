using UnityEngine;
using System.Collections;
using System;


public class Pair<U, T>
{
	public Pair()
	{
	}

	public Pair(U first, T second)
	{
		this.First = first;
		this.Second = second;
	}

	public U First { get; set; }
	public T Second { get; set; }
}



public class DungeonGenerator : MonoBehaviour {
	public GameObject oneWayGO;
	public GameObject twoWayGO;
	public GameObject twoWayCurveGO;
	public GameObject threeWayGO;
	public GameObject fourWayGO;

	public enum PartType
	{
		ONEWAY,		// => 0°, ^ 90°, <= 180°...
		TWOWAY,		// <=>
		TWOWAYCURVE,	// ^>
		THREEWAY,   // ↕>
		FOURWAY		// +
	}

	public enum Direction
	{
		NORTH=1,
		WEST=2,
		SOUTH=3,
		EAST=0
	}

	public class OutOfPart
	{
		public Part p;
		public Direction dir { get; set; }
		private Vector2 pos;

		public OutOfPart(Part p, Direction d)
		{
			float dist = 0.5f;
			this.p = p;
			dir = d;

			if (d == Direction.NORTH)
			{
				pos = new Vector2(p.x, p.y + dist);
			}
			else if (d == Direction.EAST)
			{
				pos = new Vector2(p.x + dist, p.y);
			}
			else if (d == Direction.SOUTH)
			{
				pos = new Vector2(p.x, p.y - dist);
			}
			else
			{
				pos = new Vector2(p.x - dist, p.y);
			}
		}

		public Vector2 getPosition()
		{
			return pos;
		}

		public override string ToString()
		{
			return "{" + p.x + "," + p.y + "," + dir + "}";
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() == GetType())
			{
				OutOfPart o = (OutOfPart)obj;
				return pos.x == o.pos.x && pos.y == o.pos.y ;
			}
			return false;
		}
	}

	public class LinkOfParts
	{
		public OutOfPart out1;
		public OutOfPart out2;

		public LinkOfParts(OutOfPart o1, OutOfPart o2)
		{
			out1 = o1;
			out2 = o2;
		}
	}

	abstract public class Part
	{
		protected bool[] dir = new bool[4];
		public PartType type { get; set; }
		public int x { get; set; }
		public int y { get; set; }

		public void close(Direction d)
		{
			dir[(int)d] = false;
		}

		public Part Clone()
		{
			Part p = new FourWayPart();
			p.x = x;
			p.y = y;
			p.type = type;
			for (int i = 0; i < 4; ++i) p.dir[i] = dir[i];

			return p;
		}

		public bool compareWith(Part p, Direction d)
		{
			if (p == null)
				return true;

			if (d == Direction.NORTH)
			{
				return !(dir[(int)d] ^ p.isOpen(Direction.SOUTH));
			}
			else if (d == Direction.SOUTH)
			{
				return !(dir[(int)d] ^ p.isOpen(Direction.NORTH));
			}
			else if (d == Direction.EAST)
			{
				return !(dir[(int)d] ^ p.isOpen(Direction.WEST));
			}
			else
			{
				return !(dir[(int)d] ^ p.isOpen(Direction.EAST));
			}
		}

		public bool isOpen(Direction d)
		{
			return dir[(int)d];
		}

		public Vector2 getPosition()
		{
			return new Vector2(x, y);
		}

		public ArrayList getOuts()
		{
			ArrayList list = new ArrayList();

			for (int i = 0; i < 4; ++i)
			{
				if (dir[i])
				{
					list.Add(new OutOfPart(this, (Direction)i));
				}
			}

			return list;
		}

		public bool hasOutsAvailable()
		{
			return dir[0] || dir[1] || dir[2] || dir[3];
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() == GetType())
			{
				Part p = (Part)obj;
				return p.dir == dir && p.type == type && p.x == x && p.y == y;
			}
			return false;
		}

		public override string ToString()
		{
			return"{" + type + "," + x + "," + y + "}";
		}

		abstract public Direction getDirection();
    }

	public class OneWayPart: Part
	{
		public OneWayPart(Direction d)
		{
			type = PartType.ONEWAY;
			dir[(int)Direction.NORTH] = d == Direction.NORTH;
			dir[(int)Direction.SOUTH] = d == Direction.SOUTH;
			dir[(int)Direction.EAST] = d == Direction.EAST;
			dir[(int)Direction.WEST] = d == Direction.WEST;
		}

		public override Direction getDirection()
		{
			for (int i = 0; i < 4; ++i)
			{
				if (dir[i])
				{
					return (Direction)i;
				}
			}
			Debug.Log("getDirection OneWay is not oneWay");
			return Direction.NORTH;
		}
	}

	public class TwoWayPart : Part
	{
		public TwoWayPart(Direction d)
		{
			type = PartType.TWOWAY;
			dir[(int)Direction.NORTH] = dir[(int)Direction.SOUTH] = d == Direction.NORTH || d == Direction.SOUTH;
			dir[(int)Direction.EAST] = dir[(int)Direction.WEST] = !dir[(int)Direction.NORTH];
		}

		public override Direction getDirection()
		{
			if (dir[(int)Direction.NORTH])
			{
				if (UnityEngine.Random.value < 0.5f)
				{
					return Direction.NORTH;
				}
				return Direction.SOUTH;
			}
			if (UnityEngine.Random.value < 0.5f)
			{
				return Direction.EAST;
			}
			return Direction.WEST;
		}
	}

	public class TwoWayCurvePart : Part
	{
		public TwoWayCurvePart(Direction d)
		{
			type = PartType.TWOWAYCURVE;
			dir[(int)d] = dir[((int)d + 1) % 4] = true;
			dir[((int)d + 2) % 4] = dir[((int)d + 3) % 4] = false;
		}

		public override Direction getDirection()
		{
			if (dir[(int)Direction.NORTH])
			{
				if (dir[(int)Direction.EAST])
				{
					return Direction.EAST;
				}
				return Direction.NORTH;
			}
			if (dir[(int)Direction.WEST])
			{
				return Direction.WEST;
            }
			return Direction.SOUTH;
		}
	}

	public class ThreeWayPart : Part
	{
		public ThreeWayPart(Direction d)
		{
			type = PartType.THREEWAY;
			dir[(int)Direction.NORTH] = d != Direction.NORTH;
			dir[(int)Direction.SOUTH] = d != Direction.SOUTH;
			dir[(int)Direction.EAST] = d != Direction.EAST;
			dir[(int)Direction.WEST] = d != Direction.WEST;
		}

		public override Direction getDirection()
		{
			if (dir[(int)Direction.NORTH])
			{
				if (dir[(int)Direction.WEST])
				{
					if (dir[(int)Direction.EAST])
					{
						return Direction.NORTH;
					}
					return Direction.WEST;
				}
				return Direction.EAST;
			}
			return Direction.SOUTH;
		}
	}

	public class FourWayPart : Part
	{
		public FourWayPart()
		{
			type = PartType.FOURWAY;
			dir[(int)Direction.NORTH] = 
				dir[(int)Direction.SOUTH] = 
				dir[(int)Direction.EAST] = 
				dir[(int)Direction.WEST] = true;
		}

		public override Direction getDirection()
		{
			return (Direction)UnityEngine.Random.Range(0, 4);
		}
	}

	public class DungeonMap
	{
		private Part[,] map;
		private DungeonGenerator dg;
		public int width { get; set; }
		public int height { get; set; }

		public DungeonMap(int width, int height, DungeonGenerator dg)
		{
			this.dg = dg;
			this.height = height;
			this.width = width;
			map = new Part[width, height];

			Part[] parts = getSubset();
			int x, y;

			//Randomly placing the subset of parts
			int r = 0;
			for (int i = 0; i < parts.Length; ++i)
			{
				r = 0;
				do
				{
					x = UnityEngine.Random.Range(0, width);
					y = UnityEngine.Random.Range(0, height);
					++r;
					if (r > 100)
					{
						parts[i] = getRandomPart();
					}
				}
				while (map[x, y] != null || (map[x, y] == null && !insertNoLinks(parts[i], x, y)));
			}

			//Link the different part of the subset
			ArrayList partsList = new ArrayList();
			ArrayList links = new ArrayList();
			Part[] partsCopy = new Part[parts.Length];
			for (int i = 0; i < partsCopy.Length; ++i)
				partsCopy[i] = parts[i].Clone();

			bool linked, searchOnlyPerfect = true;
			ArrayList otherParts = new ArrayList();
			ArrayList outList = partsCopy[0].getOuts();
			ArrayList outListNotPerfect = new ArrayList();

			partsList.Add(partsCopy[0]);
			while (outList.Count > 0)
			{
				int id = UnityEngine.Random.Range(0, outList.Count);
				OutOfPart currentOut = (OutOfPart)outList[id];
				outList.RemoveAt(id);

				linked = false;
				otherParts = getOtherParts(partsCopy, currentOut.p);

				while (!linked)
				{
					if (otherParts.Count == 0)
					{
						Debug.Log("No other parts found");
						break;
					}
					OutOfPart bestOut = SearchCloserOut(otherParts, currentOut, searchOnlyPerfect);
					if (bestOut == null)
					{
						outListNotPerfect.Add(currentOut);
						break;
					}
					bestOut.p.close(bestOut.dir);

					if (!partsList.Contains(bestOut.p))
					{
						ArrayList outsOfBestPart = bestOut.p.getOuts();
						outList.AddRange(outsOfBestPart);

						partsList.Add(bestOut.p);
                        Debug.Log("Add part " + bestOut.p.ToString() + " with " + outsOfBestPart.Count + " outs. " + searchOnlyPerfect);
						searchOnlyPerfect = true;
					}

					int minOk = 0;
					if (outList.Contains(bestOut))
					{
						minOk = 1;
					}

					if (outList.Count > minOk || partsList.Count == partsCopy.Length)
					{
						links.Add(new LinkOfParts(currentOut, bestOut));
						outList.Remove(bestOut);
						currentOut.p.close(currentOut.dir);
						linked = true;

						Debug.Log("Add link " + bestOut.ToString() + " and c: " + currentOut.ToString());
					}
					else
					{
						otherParts.Remove(bestOut.p);	
					}
				}

				if (outList.Count == 0)
				{
					if (partsList.Count != partsCopy.Length && outListNotPerfect.Count != 0)
					{
						outList.AddRange(outListNotPerfect);
						outListNotPerfect.RemoveRange(0, outListNotPerfect.Count);
						searchOnlyPerfect = false;
						Debug.Log("Make unperfect links from now with " + outList.Count + " outs");
					}
					else
					{
						Debug.Log("Every parts linked.");
						break;
					}
				}
			}
		}

		public ArrayList getOtherParts(Part[] partsCopy, Part p)
		{
			ArrayList others = new ArrayList();

			foreach (Part p2 in partsCopy)
			{
				if ((p.x != p2.x || p.y != p2.y) && p2.hasOutsAvailable())
				{
					others.Add(p2);
				}
			}

			return others;
		}

		public OutOfPart SearchCloserOut(ArrayList parts, OutOfPart o, bool onlyPerfect = false)
		{
			ArrayList bestParts = new ArrayList();

			if (onlyPerfect)
			{
				Vector2 dirCurrent = o.getPosition() - o.p.getPosition();
				Vector2 dir;
				

				foreach (Part p in parts)
				{
					//ArrayList outsOfP necessary? overcost?
					dir = p.getPosition() - o.getPosition();

					if (Vector2.Dot(dirCurrent, dir) > 0)
					{
						bestParts.Add(p);
						//Debug.Log("Best for " + o + " is " + p);
					}
				}

				if (bestParts.Count == 0)
				{
					Debug.Log("No perfect bestPart found");
					return null;
				}
			}
			else
			{
				bestParts = parts;
			}

			float bestDist = float.MaxValue;
			OutOfPart bestOut = null;

			foreach (Part p in bestParts)
			{
				ArrayList outsOfP = p.getOuts();
				foreach (OutOfPart outP in outsOfP)
				{
					float dist = (outP.getPosition() - o.getPosition()).SqrMagnitude();
                    if (dist < bestDist)
					{
						bestOut = outP;
						bestDist = dist;
					}
				}
			}

			if (bestOut == null)
			{
				Debug.Log("No bestOut found.");
			}

			return bestOut;
		}

		public Part getRandomPart()
		{
			PartType type = (PartType)UnityEngine.Random.Range(1, 5);

			if (type == PartType.TWOWAY)
			{
				return new TwoWayPart((Direction)UnityEngine.Random.Range(0, 4));
			}
			else if (type == PartType.TWOWAYCURVE)
			{
				return new TwoWayCurvePart((Direction)UnityEngine.Random.Range(0, 4));
			}
			else if (type == PartType.THREEWAY)
			{
				return new ThreeWayPart((Direction)UnityEngine.Random.Range(0, 4));
			}
			else
			{
				return new FourWayPart();
			}
		}

		public Part[] getSubset()
		{
			int nbCells = (height - 1) * (width - 1);
			int min = (int)Math.Sqrt(nbCells), max = (int)Math.Sqrt(nbCells) + 1;//nbCells / 2;
            Part[] parts = new Part[UnityEngine.Random.Range(min, max)];

			for (int i = 0; i < parts.Length; ++i)
			{
				parts[i] = getRandomPart();
			}

			return parts;
		}

		public bool insertNoLinks(Part p, int x, int y)
		{
			if ((x > 0 && map[x - 1, y] != null)
				|| (x - 1 < 0 && p.isOpen(Direction.WEST)))
			{
				return false;
			}
			if ((y > 0 && map[x, y - 1] != null)
				|| (y - 1 < 0 && p.isOpen(Direction.SOUTH)))
			{
				return false;
			}
			if ((x < width - 1 && map[x + 1, y] != null)
				|| (x + 1 >= width && p.isOpen(Direction.EAST)))
			{
				return false;
			}
			if ((y < height - 1 && map[x, y + 1] != null)
				|| (y + 1 >= height && p.isOpen(Direction.NORTH)))
			{
				return false;
			}

			map[x, y] = p;
			p.x = x;
			p.y = y;
			return true;
		}

		public bool insert(Part p, int x, int y)
		{
			if (( x - 1 >= 0 && !p.compareWith(map[x - 1, y], Direction.WEST) )
				|| ( x - 1 < 0 && p.isOpen(Direction.WEST) ))
			{
				return false;
			}
			if ((y - 1 >= 0 && !p.compareWith(map[x, y - 1], Direction.NORTH))
				|| (y - 1 < 0 && p.isOpen(Direction.NORTH)))
			{
				return false;
			}
			if ((x + 1 < width && !p.compareWith(map[x + 1, y], Direction.EAST))
				|| (x + 1 >= width && p.isOpen(Direction.EAST)))
			{
				return false;
			}
			if ((y + 1 < height && !p.compareWith(map[x, y + 1], Direction.SOUTH))
				|| (y + 1 >= height && p.isOpen(Direction.SOUTH)))
			{
				return false;
			}

			map[x, y] = p;
			p.x = x;
			p.y = y;
			return true;
		}

		public void instantiatePart(Part p)
		{
			if (p == null)
			{
				return;
			}

			int WIDTHPART = 10;

			if (p.type == PartType.ONEWAY)
			{
				Instantiate(dg.oneWayGO,
							new Vector3(p.x * WIDTHPART, 0, p.y * WIDTHPART),
                            Quaternion.Euler(0, -(int)p.getDirection() * 90, 0));
			} else if (p.type == PartType.TWOWAY)
			{
				Instantiate(dg.twoWayGO,
							new Vector3(p.x * WIDTHPART, 0, p.y * WIDTHPART),
							Quaternion.Euler(0, -(int)p.getDirection() * 90, 0));
			} else if (p.type == PartType.THREEWAY)
			{
				Instantiate(dg.threeWayGO,
							new Vector3(p.x * WIDTHPART, 0, p.y * WIDTHPART),
							Quaternion.Euler(0, -(int)p.getDirection() * 90, 0));
			} else if (p.type == PartType.TWOWAYCURVE)
			{
				Instantiate(dg.twoWayCurveGO,
							new Vector3(p.x * WIDTHPART, 0, p.y * WIDTHPART),
							Quaternion.Euler(0, -(int)p.getDirection() * 90, 0));
			} else
			{
				Instantiate(dg.fourWayGO,
							new Vector3(p.x * WIDTHPART, 0, p.y * WIDTHPART),
							Quaternion.Euler(0, -(int)p.getDirection() * 90, 0));
			}
		}

		public void display()
		{
			for (int x = 0; x < width; ++x)
			{
				for (int y = 0; y < height; ++y)
				{
					instantiatePart(map[x, y]);
				}
			}
		}
	}

	void Start()
	{
		DungeonMap m = new DungeonMap(5, 5, this);
		m.display();
		
	}

	public void generate(Vector2 size)
	{

	}

}
