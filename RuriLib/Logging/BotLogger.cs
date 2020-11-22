﻿using RuriLib.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RuriLib.Logging
{
    public class BotLogger : IBotLogger
    {
        private readonly List<BotLoggerEntry> entries = new List<BotLoggerEntry>();
        public event EventHandler<BotLoggerEntry> NewEntry;
        public IEnumerable<BotLoggerEntry> Entries => entries;

        public void Log(string message, string color = "#fff", bool canViewAsHtml = false)
        {
            var entry = new BotLoggerEntry
            {
                Message = message,
                Color = color,
                CanViewAsHtml = canViewAsHtml
            };

            entries.Add(entry);
            NewEntry?.Invoke(this, entry);
        }

        public void Log(IEnumerable<string> enumerable, string color = "#fff", bool canViewAsHtml = false)
        {
            var entry = new BotLoggerEntry
            {
                Message = string.Join(Environment.NewLine, enumerable),
                Color = color,
                CanViewAsHtml = canViewAsHtml
            };

            entries.Add(entry);
            NewEntry?.Invoke(this, entry);
        }

        public void LogHeader([CallerMemberName] string caller = null)
        {
            var callingMethod = new StackFrame(1).GetMethod();
            var attribute = callingMethod.GetCustomAttribute<Block>();

            if (attribute != null && attribute.name != null)
                caller = attribute.name;

            var entry = new BotLoggerEntry
            {
                Message = $">> {caller} <<",
                Color = LogColors.ChromeYellow
            };

            entries.Add(entry);
            NewEntry?.Invoke(this, entry);
        }

        public void Clear()
        {
            entries.Clear();
        }
    }
}