﻿//-----------------------------------------------------------------------
// <copyright file="OpenIdCoordinator.cs" company="Andrew Arnott">
//     Copyright (c) Andrew Arnott. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace DotNetOpenAuth.Test.OpenId {
	using System;
	using DotNetOpenAuth.Messaging.Bindings;
	using DotNetOpenAuth.OpenId;
	using DotNetOpenAuth.OpenId.Provider;
	using DotNetOpenAuth.OpenId.RelyingParty;
	using DotNetOpenAuth.Test.Mocks;

	internal class OpenIdCoordinator : CoordinatorBase<OpenIdRelyingParty, OpenIdProvider> {
		internal OpenIdCoordinator(Action<OpenIdRelyingParty> rpAction, Action<OpenIdProvider> opAction)
			: base(rpAction, opAction) {
		}

		internal OpenIdProvider Provider { get; set; }

		internal OpenIdRelyingParty RelyingParty { get; set; }

		internal override void Run() {
			this.EnsurePartiesAreInitialized();
			var rpCoordinatingChannel = new CoordinatingChannel(this.RelyingParty.Channel, this.IncomingMessageFilter, this.OutgoingMessageFilter);
			var opCoordinatingChannel = new CoordinatingChannel(this.Provider.Channel, this.IncomingMessageFilter, this.OutgoingMessageFilter);
			rpCoordinatingChannel.RemoteChannel = opCoordinatingChannel;
			opCoordinatingChannel.RemoteChannel = rpCoordinatingChannel;

			this.RelyingParty.Channel = rpCoordinatingChannel;
			this.Provider.Channel = opCoordinatingChannel;

			RunCore(this.RelyingParty, this.Provider);
		}

		private void EnsurePartiesAreInitialized() {
			if (this.RelyingParty == null) {
				this.RelyingParty = new OpenIdRelyingParty(new AssociationMemoryStore<Uri>(), new NonceMemoryStore(TimeSpan.FromHours(3)), new PrivateSecretMemoryStore());
			}

			if (this.Provider == null) {
				this.Provider = new OpenIdProvider(new AssociationMemoryStore<AssociationRelyingPartyType>(), new NonceMemoryStore(TimeSpan.FromHours(3)));
			}
		}
	}
}
