﻿/*
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

using DOL.Database.Attributes;

namespace DOL.Database
{
    public class DBLanguageItem : DataObject
    {
        private string m_id;
        private string m_examineArticle;
        private string m_name;
        private string m_language;

        public DBLanguageItem() { }

        /// <summary>
        /// The translation id
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public string TranslationId
        {
            get { return m_id; }
            set
            {
                Dirty = true;
                m_id = value;
            }
        }

        /// <summary>
        /// The translated item examine article
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public string ExamineArticle
        {
            get { return m_examineArticle; }
            set
            {
                Dirty = true;
                m_examineArticle = value;
            }
        }

        /// <summary>
        /// The translated item name
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public string Name
        {
            get { return m_name; }
            set
            {
                Dirty = true;
                m_name = value;
            }
        }

        /// <summary>
        /// The language
        /// </summary>
        [DataElement(AllowDbNull = false)]
        public string Language
        {
            get { return m_language; }
            set
            {
                Dirty = true;
                m_language = value;
            }
        }
    }
}