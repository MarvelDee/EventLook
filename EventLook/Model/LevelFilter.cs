﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventLook.Model
{
    public class LevelFilter : FilterBase
    {
        public LevelFilter()
        {
            levelFilters = new ObservableCollection<LevelFilterItem>();
            LevelFilters = new ReadOnlyObservableCollection<LevelFilterItem>(levelFilters);
        }

        /// <summary>
        /// Collection of the checkboxes to filter by event level
        /// </summary>
        private readonly ObservableCollection<LevelFilterItem> levelFilters;
        public ReadOnlyObservableCollection<LevelFilterItem> LevelFilters
        {
            get;
            private set;
        }

        public override void Refresh(IEnumerable<EventItem> events)
        {
            // Make a copy before clearing
            var prevFilters = levelFilters.Select(f => new LevelFilterItem { Level = f.Level, Selected = f.Selected }).ToList();
            levelFilters.Clear();

            // Check null, just in case.
            if (events.Select(e => e.Record.Level).Any(lv => !lv.HasValue))
            {
                levelFilters.Add(new LevelFilterItem
                {
                    Level = null,
                    Selected = prevFilters.FirstOrDefault(f => f.Level == null)?.Selected ?? true
                });
            }

            var distinctLevels = events.Select(e => e.Record.Level).Where(lv => lv.HasValue).Distinct().OrderBy(lv => lv);
            foreach (var lv in distinctLevels)
            {
                levelFilters.Add(new LevelFilterItem
                {
                    Level = lv,
                    Selected = prevFilters.FirstOrDefault(f => f.Level == lv)?.Selected ?? true
                });
            }

            Apply();
        }
        public override void Reset()
        {
            RemoveFilter();
            foreach (var lf in levelFilters)
            {
                lf.Selected = true;
            }
        }

        protected override bool IsFilterMatched(EventItem evt)
        {
            return LevelFilters.Where(lf => lf.Selected).Any(lf => lf.Level == evt.Record.Level);
        }
    }

    public class LevelFilterItem : Monitorable
    {
        private byte? level;
        public byte? Level
        {
            get { return level; }
            set
            {
                if (value == level)
                    return;

                level = value;
                NotifyPropertyChanged();
            }
        }

        private bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (value == _selected)
                    return;

                _selected = value;
                NotifyPropertyChanged();
            }
        }

        public override string ToString()
        {
            if (!level.HasValue)
                return "Invalid level";

            // In the Event Viewer, Level 0 is shown as Information.
            return (level.Value == 1) ? "Critical" :
                (level.Value == 2) ? "Error" :
                (level.Value == 3) ? "Warning" :
                (level.Value == 4) ? "Information" :
                (level.Value == 5) ? "Verbose" :
                "Unknown level";
        }
    }
}
