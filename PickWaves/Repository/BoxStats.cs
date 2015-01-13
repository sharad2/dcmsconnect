﻿using DcmsMobile.PickWaves.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DcmsMobile.PickWaves.Repository
{
    internal enum PiecesKind
    {
        Expected,
        Current
    }

    internal class BoxStats
    {
        //private readonly int?[,] _pieces;

        //[Obsolete]
        //private readonly int?[] _counts;
        //[Obsolete]
        //private const int INDEX_STATE_VERIFIED = 0;
        //[Obsolete]
        //private const int INDEX_STATE_UNVERIFIED = 1;
        //[Obsolete]
        //private const int INDEX_STATE_CANCELLED = 2;
        //[Obsolete]
        //private const int INDEX_STATE_NOTSTARTED = 3;
        //[Obsolete]
        //private const int INDEX_STATE_MAX = 3;

        private readonly IDictionary<BoxState, int?> _dictBoxCounts;

        private readonly IDictionary<Tuple<BoxState, PiecesKind>, int?> _dictPieces;

        public BoxStats()
        {
            //_pieces = new int?[INDEX_STATE_MAX + 1, 2];
            //_counts = new int?[INDEX_STATE_MAX + 1];
            _dictBoxCounts = new Dictionary<BoxState, int?>(8);
            _dictPieces = new Dictionary<Tuple<BoxState, PiecesKind>, int?>(8);
        }

        public int? this[params BoxState[] states]
        {
            get
            {
                return _dictBoxCounts.Where(p => states.Contains(p.Key)).Sum(p => p.Value);
            }
            set
            {
                if (states.Length != 1)
                {
                    throw new ArgumentOutOfRangeException("Can set the value of only one state");
                }
                _dictBoxCounts[states[0]] = value;
            }
        }

        ///// <summary>
        ///// Returns box counts
        ///// </summary>
        ///// <param name="states"></param>
        ///// <returns></returns>
        //[Obsolete]
        //public int? this[BoxState states]
        //{
        //    get
        //    {
        //        //var results = new List<int?>(INDEX_STATE_MAX);
        //        //if (states.HasFlag(BoxState.Cancelled))
        //        //{
        //        //    results.Add(_counts[INDEX_STATE_CANCELLED]);
        //        //}
        //        //if (states.HasFlag(BoxState.InProgress))
        //        //{
        //        //    results.Add(_counts[INDEX_STATE_UNVERIFIED]);
        //        //}
        //        //if (states.HasFlag(BoxState.Completed))
        //        //{
        //        //    results.Add(_counts[INDEX_STATE_VERIFIED]);
        //        //}
        //        //if (states.HasFlag(BoxState.NotStarted))
        //        //{
        //        //    results.Add(_counts[INDEX_STATE_NOTSTARTED]);
        //        //}
        //        //return results.Sum();

        //        throw new NotImplementedException();
        //    }
        //    set
        //    {
        //        //switch (states)
        //        //{
        //        //    case BoxState.Completed:
        //        //        _counts[INDEX_STATE_VERIFIED] = value;
        //        //        break;
        //        //    case BoxState.Cancelled:
        //        //        _counts[INDEX_STATE_CANCELLED] = value;
        //        //        break;

        //        //    case BoxState.InProgress:
        //        //        _counts[INDEX_STATE_UNVERIFIED] = value;
        //        //        break;

        //        //    case BoxState.NotStarted:
        //        //        _counts[INDEX_STATE_NOTSTARTED] = value;
        //        //        break;

        //        //    default:
        //        //        throw new NotSupportedException(states.ToString());
        //        //}
        //        throw new NotImplementedException();

        //    }
        //}

        /// <summary>
        /// Returns sum of pieces in boxes
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="states"></param>
        /// <returns></returns>
        public int? this[PiecesKind kind, params BoxState[] states]
        {
            get
            {
                return _dictPieces.Where(p => states.Contains(p.Key.Item1) && p.Key.Item2 == kind).Sum(p => p.Value);
                //var results = new List<int?>(2);
                //if (states.HasFlag(BoxState.Cancelled))
                //{
                //    results.Add(_pieces[INDEX_STATE_CANCELLED, (int)kind]);
                //}
                //if (states.HasFlag(BoxState.InProgress))
                //{
                //    results.Add(_pieces[INDEX_STATE_UNVERIFIED, (int)kind]);
                //}
                //if (states.HasFlag(BoxState.Completed))
                //{
                //    results.Add(_pieces[INDEX_STATE_VERIFIED, (int)kind]);
                //}
                //return results.Sum();
            }
            set
            {
                if (states.Length != 1)
                {
                    throw new ArgumentOutOfRangeException("Can set the value of only one state");
                }
                _dictPieces[Tuple.Create(states[0], kind)] = value;
                //switch (states)
                //{
                //    case BoxState.Completed:
                //        _pieces[INDEX_STATE_VERIFIED, (int)kind] = value;
                //        break;
                //    case BoxState.Cancelled:
                //        _pieces[INDEX_STATE_CANCELLED, (int)kind] = value;
                //        break;

                //    case BoxState.InProgress:
                //        _pieces[INDEX_STATE_UNVERIFIED, (int)kind] = value;
                //        break;

                //    default:
                //        throw new NotSupportedException();
                //}
            }
        }
    }
}