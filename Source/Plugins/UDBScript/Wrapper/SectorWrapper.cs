﻿#region ================== Copyright (c) 2021 Boris Iwanski

/*
 * This program is free software: you can redistribute it and/or modify
 *
 * it under the terms of the GNU General Public License as published by
 * 
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 * 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.If not, see<http://www.gnu.org/licenses/>.
 */

#endregion

#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.UDBScript.Wrapper
{
	class SectorWrapper : MapElementWrapper
	{
		#region ================== Variables

		private Sector sector;

		#endregion

		#region ================== Properties

		/// <summary>
		/// Floor height of the sector.
		/// </summary>
		public int floorHeight
		{
			get
			{
				if (sector.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the floorHeight property can not be accessed.");

				return sector.FloorHeight;
			}
			set
			{
				if (sector.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the floorHeight property can not be accessed.");

				sector.FloorHeight = value;
			}
		}

		/// <summary>
		/// Ceiling height of the sector.
		/// </summary>
		public int ceilingHeight
		{
			get
			{
				if (sector.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the ceilingHeight property can not be accessed.");

				return sector.CeilHeight;
			}
			set
			{
				if (sector.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the ceilingHeight property can not be accessed.");

				sector.CeilHeight = value;
			}
		}

		/// <summary>
		/// Floor texture of the sector.
		/// </summary>
		public string floorTexture
		{
			get
			{
				if (sector.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the floorTexture property can not be accessed.");

				return sector.FloorTexture;
			}
			set
			{
				if (sector.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the floorTexture property can not be accessed.");

				sector.SetFloorTexture(value);

				// Make sure to update the used textures, so that they are shown immediately
				General.Map.Data.UpdateUsedTextures();
			}
		}

		/// <summary>
		/// Ceiling texture of the sector.
		/// </summary>
		public string ceilingTexture
		{
			get
			{
				if (sector.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the ceilingTexture property can not be accessed.");

				return sector.CeilTexture;
			}
			set
			{
				if (sector.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the ceilingTexture property can not be accessed.");

				sector.SetCeilTexture(value);

				// Make sure to update the used textures, so that they are shown immediately
				General.Map.Data.UpdateUsedTextures();
			}
		}

		/// <summary>
		/// If the sector is selected or not
		/// </summary>
		public bool selected
		{
			get
			{
				if (sector.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the selected property can not be accessed.");

				return sector.Selected;
			}
			set
			{
				if (sector.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the selected property can not be accessed.");

				sector.Selected = value;
			}
		}

		/// <summary>
		/// If the sector is marked or not. It is used to mark map elements that were created or changed (for example after drawing new geometry).
		/// </summary>
		public bool marked
		{
			get
			{
				if (sector.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the marked property can not be accessed.");

				return sector.Marked;
			}
			set
			{
				if (sector.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the marked property can not be accessed.");

				sector.Marked = value;
			}
		}

		/// <summary>
		/// Sector flags. It's an object with the flags as properties. Only available in UDMF
		///
		/// ```
		/// s.flags['noattack'] = true; // Monsters in this sector don't attack
		/// s.flags.noattack = true; // Also works
		/// ```
		/// </summary>
		public ExpandoObject flags
		{
			get
			{
				if (sector.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the flags property can not be accessed.");

				dynamic eo = new ExpandoObject();
				IDictionary<string, object> o = eo as IDictionary<string, object>;

				foreach (KeyValuePair<string, bool> kvp in sector.GetFlags())
				{
					o.Add(kvp.Key, kvp.Value);
				}

				// Create event that gets called when a property is changed. This sets the flag
				((INotifyPropertyChanged)eo).PropertyChanged += new PropertyChangedEventHandler((sender, ea) => {
					PropertyChangedEventArgs pcea = ea as PropertyChangedEventArgs;
					IDictionary<string, object> so = sender as IDictionary<string, object>;

					// Only allow known flags to be set
					if (!General.Map.Config.SectorFlags.Keys.Contains(pcea.PropertyName))
						throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Flag name '" + pcea.PropertyName + "' is not valid.");

					// New value must be bool
					if (!(so[pcea.PropertyName] is bool))
						throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Flag values must be bool.");

					sector.SetFlag(pcea.PropertyName, (bool)so[pcea.PropertyName]);
				});

				return eo;
			}
		}

		/// <summary>
		/// The sector's special type. 
		/// </summary>
		public int special
		{
			get
			{
				if (sector.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the special property can not be accessed.");

				return sector.Effect;
			}
			set
			{
				if (sector.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the special property can not be accessed.");

				sector.Effect = value;
			}
		}

		/// <summary>
		/// The sector's tag.
		/// </summary>
		public int tag
		{
			get
			{
				if (sector.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the tag property can not be accessed.");

				return sector.Tag;
			}
			set
			{
				if (sector.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the tag property can not be accessed.");

				sector.Tag = value;
			}
		}

		/// <summary>
		/// The sector's brightness.
		/// </summary>
		public int brightness
		{
			get
			{
				if (sector.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the brightness property can not be accessed.");

				return sector.Brightness;
			}
			set
			{
				if (sector.IsDisposed)
					throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the brightness property can not be accessed.");

				sector.Brightness = value;
			}
		}

		#endregion

		#region ================== Constructors

		internal SectorWrapper(Sector sector) : base(sector)
		{
			this.sector = sector;
		}

		#endregion

		#region ================== Update

		internal override void AfterFieldsUpdate()
		{
			sector.UpdateFloorSurface();
			sector.UpdateCeilingSurface();
		}

		#endregion

		#region ================== Methods

		/// <summary>
		/// Returns an array of all sidedefs of the sector
		/// </summary>
		/// <returns></returns>
		public SidedefWrapper[] getSidedefs()
		{
			if (sector.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the getSidedefs method can not be accessed.");

			List<SidedefWrapper> sidedefs = new List<SidedefWrapper>(sector.Sidedefs.Count);

			foreach (Sidedef sd in sector.Sidedefs)
				if (!sd.IsDisposed)
					sidedefs.Add(new SidedefWrapper(sd));

			return sidedefs.ToArray();
		}

		/// <summary>
		/// Clears all flags.
		/// </summary>
		public void clearFlags()
		{
			if (sector.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the cleaFlags method can not be accessed.");

			sector.ClearFlags();
		}

		/// <summary>
		/// Copies the properties from this sector to another.
		/// </summary>
		/// <param name="s">the sector to copy the properties to</param>
		public void copyPropertiesTo(SectorWrapper s)
		{
			if (sector.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the cleaFlags method can not be accessed.");

			sector.CopyPropertiesTo(s.sector);
		}

		/// <summary>
		/// Checks if the given point is in this sector or not. The given point can be a `Vector2D` or an `Array` of two numbers.
		/// ```
		/// if(s.intersect(new Vector2D(32, 64)))
		///		log('Point is in the sector!');
		///		
		/// if(s.intersect([ 32, 64 ]))
		///		log('Point is in the sector!');
		///	```
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public bool intersect(object p)
		{
			if (sector.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the intersect method can not be accessed.");

			if (p is Vector2D)
				return sector.Intersect((Vector2D)p);
			if(p.GetType().IsArray)
			{
				object[] vals = (object[])p;

				// Make sure all values in the array are doubles
				foreach (object v in vals)
					if (!(v is double))
						throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Values in position array must be numbers.");

				if (vals.Length == 2)
					return sector.Intersect(new Vector2D((double)vals[0], (double)vals[1]));

				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Position array must contain 2 values.");
			}

			throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Position values must be a Vector2D, or an array of numbers.");
		}

		/// <summary>
		/// Joins this sector with another sector. Lines shared between the sectors will not be removed.
		/// </summary>
		/// <param name="other">Sector to join with</param>
		public void join(Sector other)
		{
			if (sector.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector is disposed, the join method can not be accessed.");

			if(other.IsDisposed)
				throw BuilderPlug.Me.ScriptRunner.CreateRuntimeException("Sector to join with is disposed, the join method can not be used.");

			sector.Join(other);
		}

		#endregion
	}
}
