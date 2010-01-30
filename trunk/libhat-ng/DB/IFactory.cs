/*
 * Created by SharpDevelop.
 * User: savant
 * Date: 06.12.2009
 * Time: 17:29
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace libhat_ng.DB
{
	/// <summary>
	/// Interface for typized object constructors
	/// </summary>
	public interface IFactory<T>
	{
		void Save(T obj);
        IList<T> Load( object criteria );
		T LoadOne(object criteria);
		void Remove(T obj );
	}

    public interface ICriteria
    {
        
    }

    [Flags]
    public enum CriteriaOperations
    {
        Equal,
        NotEqual,
        More,
        Less
    }
}
