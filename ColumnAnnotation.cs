﻿using System;
using System.Collections.Generic;
using System.IO;
using voidsoft.efbog;

namespace voidsoft
{
	//sample notation : [TableName]/[FieldName]/[value]

	//Profesionist||Poza||file
	//Profesionist||Telefon||enum=1|A,2|B,3|DelaGROS,4|FFFFF
	//Profesionist||Poza||length=50

	public class ColumnAnnotation
	{
		public string EntityName;
		public Dictionary<int, string> EnumValues = new Dictionary<int, string>();
		public int FieldLength;

		public bool HoldsFile;

		public bool IsEnum;

		public string PropertyName;

		public static bool IsFileType(EntityData e, EntityProperty p)
		{
			List<ColumnAnnotation> annotations = GeneratorContext.Annotations.FindAll(annotation => annotation.EntityName == e.Entity.Name);

			ColumnAnnotation column = annotations.Find(a => a.PropertyName == p.PropertyName);

			if (column != null)
			{
				return column.HoldsFile;
			}

			return false;
		}

		public static bool IsEnumType(EntityData e, EntityProperty p, ref Dictionary<int, string> values)
		{
			List<ColumnAnnotation> annotations = GeneratorContext.Annotations.FindAll(annotation => annotation.EntityName == e.Entity.Name);

			ColumnAnnotation column = annotations.Find(a => a.PropertyName == p.PropertyName);

			if (column != null)
			{
				if (column.IsEnum)
				{
					values = column.EnumValues;
					return true;
				}
			}

			return false;
		}

		public static int GetFieldLength(EntityData e, EntityProperty p)
		{
			List<ColumnAnnotation> annotations = GeneratorContext.Annotations.FindAll(annotation => annotation.EntityName == e.Entity.Name);

			ColumnAnnotation column = annotations.Find(a => a.PropertyName == p.PropertyName);

			if (column != null)
			{
				return column.FieldLength;
			}

			return -1;
		}

		public static List<ColumnAnnotation> ParseAnnotations(string fileName)
		{
			if (!File.Exists(fileName))
			{
				return null;
			}

			var list = new List<ColumnAnnotation>();

			string[] strings = File.ReadAllLines(fileName);

			foreach (var line in strings)
			{
				try
				{
					string[] split = line.Split(new[] {"||"}, StringSplitOptions.RemoveEmptyEntries);

					if (split.Length != 3)
					{
						continue;
					}

					var a = new ColumnAnnotation();
					a.EntityName = split[0];
					a.PropertyName = split[1];

					string lastPart = split[2].ToLower();

					if (lastPart == "file")
					{
						a.HoldsFile = true;
					}

					if (lastPart.StartsWith("enum"))
					{
						//remove the start
						lastPart = lastPart.Substring(6);

						string[] enumParts = lastPart.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);

						foreach (var s in enumParts)
						{
							try
							{
								string[] pieces = s.Split(',');

								if (pieces.Length == 2)
								{
									a.EnumValues.Add(Convert.ToInt32(pieces[0]), pieces[1]);
								}
							}
							catch
							{
								continue;
							}
						}
					}

					if (lastPart.StartsWith("length"))
					{
						string[] length = lastPart.Split(new[] {"="}, StringSplitOptions.RemoveEmptyEntries);
						a.FieldLength = Convert.ToInt32(length[1]);
					}

					list.Add(a);
				}
				catch
				{
					continue;
				}
			}

			return list;
		}
	}
}