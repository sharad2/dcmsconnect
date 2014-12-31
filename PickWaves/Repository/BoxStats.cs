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
        private readonly int?[,] _pieces;

        private readonly int?[] _counts;
        private const int INDEX_STATE_VERIFIED = 0;
        private const int INDEX_STATE_UNVERIFIED = 1;
        private const int INDEX_STATE_CANCELLED = 2;
        private const int INDEX_STATE_NOTSTARTED = 3;
        private const int INDEX_STATE_MAX = 3;

        public BoxStats()
        {
            _pieces = new int?[INDEX_STATE_MAX + 1, 2];
            _counts = new int?[INDEX_STATE_MAX + 1];
        }


        /// <summary>
        /// Returns box counts
        /// </summary>
        /// <param name="states"></param>
        /// <returns></returns>
        public int? this[BoxState states]
        {
            get
            {
                var results = new List<int?>(INDEX_STATE_MAX);
                if (states.HasFlag(BoxState.Cancelled))
                {
                    results.Add(_counts[INDEX_STATE_CANCELLED]);
                }
                if (states.HasFlag(BoxState.InProgress))
                {
                    results.Add(_counts[INDEX_STATE_UNVERIFIED]);
                }
                if (states.HasFlag(BoxState.Completed))
                {
                    results.Add(_counts[INDEX_STATE_VERIFIED]);
                }
                if (states.HasFlag(BoxState.NotStarted))
                {
                    results.Add(_counts[INDEX_STATE_NOTSTARTED]);
                }
                return results.Sum();


            }
            set
            {
                switch (states)
                {
                    case BoxState.Completed:
                        _counts[INDEX_STATE_VERIFIED] = value;
                        break;
                    case BoxState.Cancelled:
                        _counts[INDEX_STATE_CANCELLED] = value;
                        break;

                    case BoxState.InProgress:
                        _counts[INDEX_STATE_UNVERIFIED] = value;
                        break;

                    case BoxState.NotStarted:
                        _counts[INDEX_STATE_NOTSTARTED] = value;
                        break;

                    default:
                        throw new NotSupportedException(states.ToString());
                }

            }
        }

        /// <summary>
        /// Returns sum of pieces in boxes
        /// </summary>
        /// <param name="states"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        public int? this[BoxState states, PiecesKind kind]
        {
            get
            {
                var results = new List<int?>(2);
                if (states.HasFlag(BoxState.Cancelled))
                {
                    results.Add(_pieces[INDEX_STATE_CANCELLED, (int)kind]);
                }
                if (states.HasFlag(BoxState.InProgress))
                {
                    results.Add(_pieces[INDEX_STATE_UNVERIFIED, (int)kind]);
                }
                if (states.HasFlag(BoxState.Completed))
                {
                    results.Add(_pieces[INDEX_STATE_VERIFIED, (int)kind]);
                }
                return results.Sum();
            }
            set
            {
                switch (states)
                {
                    case BoxState.Completed:
                        _pieces[INDEX_STATE_VERIFIED, (int)kind] = value;
                        break;
                    case BoxState.Cancelled:
                        _pieces[INDEX_STATE_CANCELLED, (int)kind] = value;
                        break;

                    case BoxState.InProgress:
                        _pieces[INDEX_STATE_UNVERIFIED, (int)kind] = value;
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
        }
    }
}