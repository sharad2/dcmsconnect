using DcmsMobile.PickWaves.Helpers;
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

        private readonly IDictionary<BoxState, int?> _dictBoxCounts;

        private readonly IDictionary<Tuple<BoxState, PiecesKind>, int?> _dictPieces;

        public BoxStats()
        {
            _dictBoxCounts = new Dictionary<BoxState, int?>(8);
            _dictPieces = new Dictionary<Tuple<BoxState, PiecesKind>, int?>(8);
        }

        public int? this[BoxState state]
        {
            get
            {
                return _dictBoxCounts.Where(p => p.Key == state).Sum(p => p.Value);
            }
            set
            {
                _dictBoxCounts[state] = value;
            }
        }


        [Obsolete]
        public int? this[BoxState[] states]
        {
            get
            {
                return _dictBoxCounts.Where(p => states.Contains(p.Key)).Sum(p => p.Value);
            }
        }

        public int? GetBoxCounts(BoxState[] states)
        {
            return _dictBoxCounts.Where(p => states.Contains(p.Key)).Sum(p => p.Value);
        }

        /// <summary>
        /// Returns sum of pieces in boxes
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="states"></param>
        /// <returns></returns>
        [Obsolete]
        public int? this[PiecesKind kind, BoxState[] states]
        {
            get
            {
                return _dictPieces.Where(p => states.Contains(p.Key.Item1) && p.Key.Item2 == kind).Sum(p => p.Value);
            }
        }

        public int? GetPieces(PiecesKind kind, BoxState[] states)
        {
            return _dictPieces.Where(p => states.Contains(p.Key.Item1) && p.Key.Item2 == kind).Sum(p => p.Value);
        }

        public int? this[PiecesKind kind, BoxState state]
        {
            get
            {
                return _dictPieces.Where(p => p.Key.Item1 == state && p.Key.Item2 == kind).Sum(p => p.Value);
            }
            set
            {
                _dictPieces[Tuple.Create(state, kind)] = value;
            }
        }
    }
}