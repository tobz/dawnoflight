/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using DOL;
using DOL.GS;
using NUnit.Framework;

namespace DOL.Tests
{	
	[TestFixture]
	public class LanguageTest : DOLTestCase
	{
		public LanguageTest()
		{
		}
		[Test]
		public void TestGetString()
		{
			Console.WriteLine("TestGetString();");
			Console.WriteLine(LanguageMgr.GetString("test","fail default string"));
			Console.WriteLine(LanguageMgr.GetString("test2","fail default string2"));
			
			Console.WriteLine(LanguageMgr.GetString("test3","defaultstring test3 succeed"));
			
			Console.WriteLine(LanguageMgr.GetString("testscript","fail default string"));
			Console.WriteLine(LanguageMgr.GetString("test.script2","fail default string"));
		}
	}
}
