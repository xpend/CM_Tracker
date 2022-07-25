﻿using System;
using Blish_HUD;
using Blish_HUD.ArcDps;
using Microsoft.Xna.Framework;

namespace falcon.cmtracker
{
    public class ArcDpsChecker
    {

        public event EventHandler ArcDpsTimedOut;

        // To adjust as needed, eventually put it in settings?
        private const double TIMEOUT_DELAY_MILLISECONDS = 60_000;

        private readonly ArcDpsService _arcDpsService;
        private readonly OverlayService _overlayService;
        private readonly Gw2MumbleService _gw2MumbleService;

        /// <summary>
        /// <c>true</c> if the instance is currently checking, <c>false</c> otherwise.
        /// </summary>
        public bool IsEnabled { get; private set; }

        private double? _lastEventTime = null;

        public ArcDpsChecker(ArcDpsService arcDpsService, OverlayService overlayService, Gw2MumbleService gw2MumbleService)
        {
            _arcDpsService = arcDpsService;
            _overlayService = overlayService;
            _gw2MumbleService = gw2MumbleService;

            _arcDpsService.RawCombatEvent += OnCombatEvent;
            _gw2MumbleService.PlayerCharacter.IsInCombatChanged += IsInCombatChanged;

        }

        /// <summary>
        /// Start checking if ArcDPS has timed out or not.
        /// </summary>
        public void Enable()
        {
            this.IsEnabled = true;

            _lastEventTime = _gw2MumbleService.PlayerCharacter.IsInCombat switch
            {
                true => _overlayService.CurrentGameTime.TotalGameTime.TotalMilliseconds,
                false => null,
            };
        }


        public void Check(GameTime gameTime)
        {
            if (this.IsEnabled == false) return;
            if (_lastEventTime == null) return;

            var currentTime = gameTime.TotalGameTime.TotalMilliseconds;

            if (currentTime - _lastEventTime > TIMEOUT_DELAY_MILLISECONDS)
            {
                this.IsEnabled = false;
                ArcDpsTimedOut?.Invoke(this, new EventArgs());
            }
        }

        private void OnCombatEvent(object sender, RawCombatEventArgs e)
        {
            if (this.IsEnabled == false) return;
            if (_lastEventTime == null) return;

            _lastEventTime = _overlayService.CurrentGameTime.TotalGameTime.TotalMilliseconds;
        }

        private void IsInCombatChanged(object sender, ValueEventArgs<bool> e)
        {
            if (this.IsEnabled == false) return;

            _lastEventTime = e.Value switch
            {
                true => _overlayService.CurrentGameTime.TotalGameTime.TotalMilliseconds,
                false => null,
            };
        }
    }
}
