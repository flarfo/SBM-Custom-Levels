using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Catobyte.Networking.Physics.Objects;
using Catobyte.Networking.Physics.Utilities;
using UnityEngine;

namespace Catobyte.Networking.Physics.Authority
{
		[RequireComponent(typeof(NObject))]
	public class NetworkAgent : MonoBehaviour
	{
						public TickNumber RemoteTickNumber
		{
			get
			{
				return this.newestSeenTickNumber;
			}
		}

								internal event Action OnDestroyed
		{
			[CompilerGenerated]
			add
			{
				Action action = this.OnDestroyed;
				Action action2;
				do
				{
					action2 = action;
					Action action3 = (Action)Delegate.Combine(action2, value);
					action = Interlocked.CompareExchange<Action>(ref this.OnDestroyed, action3, action2);
				}
				while (action != action2);
			}
			[CompilerGenerated]
			remove
			{
				Action action = this.OnDestroyed;
				Action action2;
				do
				{
					action2 = action;
					Action action3 = (Action)Delegate.Remove(action2, value);
					action = Interlocked.CompareExchange<Action>(ref this.OnDestroyed, action3, action2);
				}
				while (action != action2);
			}
		}

				internal void DrawAuthVolumeGizmos()
		{
			for (int i = 0; i < this.authorities.Count; i++)
			{
				this.authorities[i].DrawAuthVolumeGizmos(this.debugColor);
			}
		}

				internal void DrawAuthVolumesRuntime()
		{
			for (int i = 0; i < this.authorities.Count; i++)
			{
				this.authorities[i].DrawAuthVolumeRuntime(this.debugColor);
			}
		}

				internal void DrawAuthChainGizmos()
		{
			Gizmos.color = this.debugColor;
			Gizmos.DrawWireSphere(this.root.HasRigidbody ? this.root.Rigidbody.worldCenterOfMass : this.root.transform.position, 0.0625f);
			if (this.authorities.Count >= 2)
			{
				for (int i = 0; i < this.authorities.Count; i++)
				{
					NObject nobject = this.authorities[i];
					Vector3 vector = (nobject.HasRigidbody ? nobject.Rigidbody.worldCenterOfMass : nobject.transform.position);
					Gizmos.DrawWireSphere(vector, 0.05f);
					NObject parent = nobject.Parent;
					if (parent != null && this.authorities.Contains(parent))
					{
						Vector3 vector2 = (parent.HasRigidbody ? parent.Rigidbody.worldCenterOfMass : parent.transform.position);
						Gizmos.DrawLine(vector, vector2);
					}
				}
			}
		}

				internal void DrawAuthLabels()
		{
		}

				private void Awake()
		{
			this.authChains = new AuthChainTable(16);
			this.authorities = new NList();
			this.authReleaseRequests = new NList();
			this.proxyAuthorities = new NList();
			this.conflictsToProcess = new NList();
			this.authDecs = new AuthDecList(128);
			this.recentlyInRootAuthChain = new NListTimed(this.rootAuthChainReleaseDelay);
			this.authChainRecords = new List<NetworkAgent.AuthChainRecord>(16);
			this.authChainRecordPool = new Stack<NetworkAgent.AuthChainRecord>();
			this.authChainRecords_SortedByTickNumber = new List<NetworkAgent.AuthChainRecord>(16);
			this.seenNObjectIds = new HashSet<ushort>();
			this.declaredChainsForNewestTickNumber = new List<byte>(16);
			this.receivedChainsForNewestTickNumber = new HashSet<byte>();
			this.root = base.GetComponent<NObject>();
			this.rank = new AgentRank(this);
			NetworkPhysics.OnReset += this.OnPhysicsReset;
			NetworkPhysics.OnPreStep += this.OnPrePhysicsTick;
			NObject.OnDisabled += this.OnNObjectDisabled;
		}

				private void OnDestroy()
		{
			NetworkPhysics.OnReset -= this.OnPhysicsReset;
			NetworkPhysics.OnPreStep -= this.OnPrePhysicsTick;
			NObject.OnDisabled -= this.OnNObjectDisabled;
			Action onDestroyed = this.OnDestroyed;
			if (onDestroyed == null)
			{
				return;
			}
			onDestroyed();
		}

				private void OnEnable()
		{
			if (!NetworkAgent.AllAgents.Contains(this))
			{
				NetworkAgent.AllAgents.Add(this);
			}
		}

				private void OnDisable()
		{
			NetworkAgent.AllAgents.Remove(this);
			this.ClearAuthorities();
		}

				private void OnPrePhysicsTick()
		{
			this.recentlyInRootAuthChain.ExpiryTime = this.rootAuthChainReleaseDelay;
			this.recentlyInRootAuthChain.Update(Time.fixedDeltaTime);
			if (this.type == AgentType.Remote && this.authChainRecords != null)
			{
				for (int i = this.authChainRecords.Count - 1; i >= 0; i--)
				{
					NetworkAgent.AuthChainRecord authChainRecord = this.authChainRecords[i];
					authChainRecord.Age += Time.fixedDeltaTime;
					if (authChainRecord.Age >= 0.25f)
					{
						this.RetireAuthChainRecord(authChainRecord);
						this.authChainRecords.RemoveAt(i);
						this.authDecsDirty = true;
					}
				}
			}
		}

				private void OnPhysicsReset()
		{
			this.ClearAuthorities();
		}

						public NObject Root
		{
			get
			{
				if (this.root == null)
				{
					this.root = base.GetComponent<NObject>();
				}
				return this.root;
			}
		}

								public AgentType Type
		{
			get
			{
				return this.type;
			}
			set
			{
				this.type = value;
			}
		}

						internal AgentRank Rank
		{
			get
			{
				return this.rank;
			}
		}

						internal NList Authorities
		{
			get
			{
				return this.authorities;
			}
		}

						internal NObjectId Id
		{
			get
			{
				if (!(this.root != null))
				{
					return NObjectId.None;
				}
				return this.root.Id;
			}
		}

						internal AuthDecList AuthDecs
		{
			get
			{
				return this.authDecs;
			}
		}

								public Color DebugColor
		{
			get
			{
				return this.debugColor;
			}
			set
			{
				this.debugColor = value;
			}
		}

				private void OnNObjectDisabled(NObject n)
		{
			this.authorities.Remove(n);
		}

				private void CalculateAuthChains()
		{
			this.authChains.Clear();
			AgentType agentType = this.type;
			if (agentType != AgentType.Local)
			{
				if (agentType == AgentType.Remote)
				{
					if (this.authDecsDirty)
					{
						this.authDecs.Clear();
						this.seenNObjectIds.Clear();
						this.authChainRecords_SortedByTickNumber.Clear();
						for (int i = 0; i < this.authChainRecords.Count; i++)
						{
							this.authChainRecords_SortedByTickNumber.Add(this.authChainRecords[i]);
						}
						this.authChainRecords_SortedByTickNumber.Sort(NetworkAgent.NewestRemoteTickNumberFirst.Instance);
						for (int j = 0; j < this.authChainRecords_SortedByTickNumber.Count; j++)
						{
							AuthDecList authDecList = this.authChainRecords_SortedByTickNumber[j].AuthDecs;
							for (int k = 0; k < authDecList.Count; k++)
							{
								AuthDec authDec = authDecList[k];
								if (this.seenNObjectIds.Add(authDec.NObjectId))
								{
									this.authDecs.AddCloneOf(authDec);
								}
							}
						}
						this.authDecsDirty = false;
					}
					this.authorities.Clear();
					for (int l = 0; l < this.authDecs.Count; l++)
					{
						AuthDec authDec2 = this.authDecs[l];
						int authChainIndex = authDec2.AuthChainIndex;
						if (NObject.Exists(authDec2.NObjectId))
						{
							while (this.authChains.Count < authChainIndex + 1)
							{
								this.authChains.Add();
							}
							this.authChains[authChainIndex].Add(authDec2.NObject);
						}
					}
				}
			}
			else if (this.rank.Level != AgentRankLevel.None)
			{
				NList authChain = this.root.GetAuthChain();
				this.authChains.AddCloneOf(authChain);
				this.rootAuthChain = this.authChains[0];
				for (int m = 0; m < this.authorities.Count; m++)
				{
					NObject nobject = this.authorities[m];
					if (!this.rootAuthChain.Contains(nobject))
					{
						NList authChain2 = nobject.GetAuthChain();
						this.authChains.Merge(authChain2);
					}
				}
			}
			if (this.authChains.Count == 0)
			{
				this.authChains.Add();
			}
			this.rootAuthChain = this.authChains[0];
			this.recentlyInRootAuthChain.AddRange(this.rootAuthChain);
		}

				private void CalculateAuthorities()
		{
			for (int i = this.authorities.Count - 1; i >= 0; i--)
			{
				NObject nobject = this.authorities[i];
				if (nobject.gameObject.layer != base.gameObject.layer || !NObject.IsActiveInHierarchy(nobject))
				{
					this.authorities.RemoveAt(i);
				}
			}
			this.CalculateAuthChains();
			for (int j = 0; j < this.authChains.Count; j++)
			{
				NList nlist = this.authChains[j];
				for (int k = 0; k < nlist.Count; k++)
				{
					this.authorities.Add(nlist[k]);
				}
			}
			this.authReleaseRequests.Clear();
			if (this.type == AgentType.Local)
			{
				if (this.releaseMotionlessAuths)
				{
					for (int l = this.authorities.Count - 1; l >= 0; l--)
					{
						NObject nobject2 = this.authorities[l];
						if (NObject.IsActiveInHierarchy(nobject2) && nobject2.DeauthorizeWhenMotionless && !nobject2.HasMotion && !this.rootAuthChain.Contains(nobject2))
						{
							this.authorities.RemoveAt(l);
						}
					}
				}
				for (int m = 0; m < NetworkAgent.AllAgents.Count; m++)
				{
					NetworkAgent networkAgent = NetworkAgent.AllAgents[m];
					if (networkAgent != this && networkAgent.rank > this.rank)
					{
						this.authorities.RemoveAllOf(networkAgent.root.TransformGroup);
					}
				}
			}
			if (Application.isEditor)
			{
				this.proxyAuthorities.Clear();
				for (int n = 1; n < this.authChains.Count; n++)
				{
					NList nlist2 = this.authChains[n];
					for (int num = 0; num < nlist2.Count; num++)
					{
						this.proxyAuthorities.Add(nlist2[num]);
					}
				}
			}
		}

				private void GenerateAuthorityDeclarations()
		{
			if (this.type == AgentType.Remote)
			{
				return;
			}
			this.authDecs.Clear();
			for (int i = 0; i < this.authChains.Count; i++)
			{
				NList nlist = this.authChains[i];
				for (int j = 0; j < nlist.Count; j++)
				{
					NObject nobject = nlist[j];
					if (nobject.SyncState && (this.authorities.Contains(nobject) || this.authReleaseRequests.Contains(nobject)))
					{
						AuthDec authDec = this.authDecs.Add();
						authDec.NObject = nobject;
						authDec.AuthChainIndex = i;
						nobject.GetState(authDec.State);
					}
				}
			}
		}

				private void ResolveConflictsAgainst(NetworkAgent otherAgent)
		{
			this.authorities.Intersect(otherAgent.authorities, this.conflictsToProcess);
			while (this.conflictsToProcess.Count > 0)
			{
				NObject nobject = this.conflictsToProcess[0];
				if (this.rank > otherAgent.rank)
				{
					AgentType agentType = this.type;
					if (agentType != AgentType.Local)
					{
						if (agentType != AgentType.Remote)
						{
						}
					}
					else if (otherAgent.rank.Level == AgentRankLevel.Master && otherAgent.rootAuthChain.Contains(nobject) && !this.recentlyInRootAuthChain.Contains(nobject) && !this.recentlyInRootAuthChain.ContainsAnyOf(otherAgent.root.TransformGroup))
					{
						this.< ResolveConflictsAgainst > g__ReleaseAuth | 63_0(nobject);
					}
				}
				else
				{
					this.< ResolveConflictsAgainst > g__ReleaseAuth | 63_0(nobject);
				}
				this.conflictsToProcess.Remove(nobject);
			}
		}

				internal void ReceiveRemoteAuthDecs(AuthDecList decs, TickNumber tickNumber, byte chainIndex, List<byte> declaredChainIndices, bool snap)
		{
			if (this.hasCompleteRemoteTick && tickNumber < this.newestCompleteTickNumber)
			{
				return;
			}
			if (tickNumber > this.newestSeenTickNumber)
			{
				this.newestSeenTickNumber = tickNumber;
				this.receivedChainsForNewestTickNumber.Clear();
				this.declaredChainsForNewestTickNumber.Clear();
				this.declaredChainsForNewestTickNumber.AddRange(declaredChainIndices);
				this.pendingSnap = snap;
			}
			if (decs.Count > 0)
			{
				NetworkAgent.AuthChainRecord authChainRecord = this.FindAuthChainRecord(tickNumber, chainIndex);
				if (authChainRecord == null)
				{
					authChainRecord = this.GetPooledAuthChainRecord();
					authChainRecord.RemoteTickNumber = tickNumber;
					authChainRecord.AuthChainIndex = chainIndex;
					this.authChainRecords.Add(authChainRecord);
				}
				else
				{
					authChainRecord.AuthDecs.Clear();
					authChainRecord.Age = 0f;
				}
				for (int i = 0; i < decs.Count; i++)
				{
					AuthDec authDec = decs[i];
					authDec.State.Snap = this.pendingSnap;
					authChainRecord.AuthDecs.AddCloneOf(authDec);
				}
			}
			if (tickNumber == this.newestSeenTickNumber)
			{
				this.receivedChainsForNewestTickNumber.Add(chainIndex);
				if (this.NewestTickIsComplete())
				{
					this.newestCompleteTickNumber = this.newestSeenTickNumber;
					this.hasCompleteRemoteTick = true;
					for (int j = this.authChainRecords.Count - 1; j >= 0; j--)
					{
						NetworkAgent.AuthChainRecord authChainRecord2 = this.authChainRecords[j];
						if (authChainRecord2.RemoteTickNumber < this.newestCompleteTickNumber)
						{
							this.RetireAuthChainRecord(authChainRecord2);
							this.authChainRecords.RemoveAt(j);
						}
					}
				}
			}
			this.authDecsDirty = true;
		}

				private NetworkAgent.AuthChainRecord FindAuthChainRecord(TickNumber tickNumber, byte chainIndex)
		{
			for (int i = 0; i < this.authChainRecords.Count; i++)
			{
				NetworkAgent.AuthChainRecord authChainRecord = this.authChainRecords[i];
				if (authChainRecord.RemoteTickNumber == tickNumber && authChainRecord.AuthChainIndex == chainIndex)
				{
					return authChainRecord;
				}
			}
			return null;
		}

				private NetworkAgent.AuthChainRecord GetPooledAuthChainRecord()
		{
			NetworkAgent.AuthChainRecord authChainRecord = ((this.authChainRecordPool.Count > 0) ? this.authChainRecordPool.Pop() : new NetworkAgent.AuthChainRecord());
			authChainRecord.AuthDecs.Clear();
			authChainRecord.Age = 0f;
			return authChainRecord;
		}

				private void RetireAuthChainRecord(NetworkAgent.AuthChainRecord record)
		{
			record.AuthDecs.Clear();
			this.authChainRecordPool.Push(record);
		}

				private void ResetAuthChainRecords()
		{
			if (this.authChainRecords == null)
			{
				return;
			}
			for (int i = 0; i < this.authChainRecords.Count; i++)
			{
				this.RetireAuthChainRecord(this.authChainRecords[i]);
			}
			this.authChainRecords.Clear();
			this.declaredChainsForNewestTickNumber.Clear();
			this.receivedChainsForNewestTickNumber.Clear();
			this.newestSeenTickNumber = 0U;
			this.newestCompleteTickNumber = 0U;
			this.hasCompleteRemoteTick = false;
			this.authDecsDirty = false;
		}

				private bool NewestTickIsComplete()
		{
			for (int i = 0; i < this.declaredChainsForNewestTickNumber.Count; i++)
			{
				if (!this.receivedChainsForNewestTickNumber.Contains(this.declaredChainsForNewestTickNumber[i]))
				{
					return false;
				}
			}
			return true;
		}

				public void SetRank(AgentRankLevel level)
		{
			this.rank.SetLevel(level);
			if (level == AgentRankLevel.None)
			{
				this.authorities.Clear();
			}
		}

				public void TransferAuthoritiesTo(NetworkAgent other)
		{
			other.ClearAuthorities();
			foreach (NetworkAgent networkAgent in NetworkAgent.AllAgents)
			{
				if (!(networkAgent == this) && !(networkAgent == other) && networkAgent.HasAuthorityOfAgent(this))
				{
					networkAgent.authorities.RemoveAllOf(this.root.TransformGroup);
					networkAgent.authorities.AddRange(other.root.TransformGroup);
				}
			}
			other.authorities.AddRange(this.authorities);
			other.RemoveAuthorities(this.root.TransformGroup);
			this.ClearAuthorities();
			this.SetRank(AgentRankLevel.None);
		}

				public bool HasAuthorityOf(NObject n)
		{
			return this.authorities.Contains(n);
		}

				public void RemoveAuthorities(IEnumerable<NObject> list)
		{
			foreach (NObject nobject in list)
			{
				this.authorities.Remove(nobject);
			}
		}

				public void ClearAuthorities()
		{
			this.authorities.Clear();
			this.proxyAuthorities.Clear();
			this.recentlyInRootAuthChain.Clear();
			this.authChains.Clear();
			this.authDecs.Clear();
			this.ResetAuthChainRecords();
			this.authReleaseRequests.Clear();
		}

				public bool HasAuthorityOfAgent(NetworkAgent agent)
		{
			foreach (NObject nobject in agent.root.TransformGroup)
			{
				if (this.HasAuthorityOf(nobject))
				{
					return true;
				}
			}
			return false;
		}

				public bool AuthorityOwnedByRemoteAgents()
		{
			using (List<NetworkAgent>.Enumerator enumerator = NetworkAgent.GetRemoteAgents().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.HasAuthorityOfAgent(this))
					{
						return true;
					}
				}
			}
			return false;
		}

				public bool AuthorityOwnedByLocalAgents()
		{
			return !this.AuthorityOwnedByRemoteAgents();
		}

				public void TakeAuthorityOf(NObject n)
		{
			this.authorities.Add(n);
		}

				public static NetworkAgent GetById(NObjectId id)
		{
			foreach (NetworkAgent networkAgent in NetworkAgent.AllAgents)
			{
				if (networkAgent.root.Id == id)
				{
					return networkAgent;
				}
			}
			return null;
		}

				public static NetworkAgent GetOwnerOf(NObject n)
		{
			foreach (NetworkAgent networkAgent in NetworkAgent.AllAgents)
			{
				if (networkAgent.authorities.Contains(n))
				{
					return networkAgent;
				}
			}
			return null;
		}

				public static bool LocalAgentsHaveAuthorityOf(NObject n)
		{
			for (int i = 0; i < NetworkAgent.AllAgents.Count; i++)
			{
				NetworkAgent networkAgent = NetworkAgent.AllAgents[i];
				if (networkAgent.type == AgentType.Local && networkAgent.HasAuthorityOf(n))
				{
					return true;
				}
			}
			return false;
		}

				public static bool RemoteAgentsHaveAuthorityOf(NObject n)
		{
			for (int i = 0; i < NetworkAgent.AllAgents.Count; i++)
			{
				NetworkAgent networkAgent = NetworkAgent.AllAgents[i];
				if (networkAgent.type == AgentType.Remote && networkAgent.HasAuthorityOf(n))
				{
					return true;
				}
			}
			return false;
		}

				public static NetworkAgent GetClosestAgentToLocation(Vector3 location, AgentType agentType)
		{
			float num = float.PositiveInfinity;
			NetworkAgent networkAgent = null;
			foreach (NetworkAgent networkAgent2 in NetworkAgent.AllAgents)
			{
				if (networkAgent2.type == agentType)
				{
					float num2 = Vector3.Distance(networkAgent2.transform.position, location);
					if (num2 < num)
					{
						num = num2;
						networkAgent = networkAgent2;
					}
				}
			}
			return networkAgent;
		}

						public static IReadOnlyList<NetworkAgent> All
		{
			get
			{
				return NetworkAgent.AllAgents;
			}
		}

				public static List<NetworkAgent> GetLocalAgents()
		{
			NetworkAgent.LocalAgents.Clear();
			foreach (NetworkAgent networkAgent in NetworkAgent.AllAgents)
			{
				if (networkAgent.type == AgentType.Local)
				{
					NetworkAgent.LocalAgents.Add(networkAgent);
				}
			}
			return NetworkAgent.LocalAgents;
		}

				public static List<NetworkAgent> GetRemoteAgents()
		{
			NetworkAgent.RemoteAgents.Clear();
			foreach (NetworkAgent networkAgent in NetworkAgent.AllAgents)
			{
				if (networkAgent.type == AgentType.Remote)
				{
					NetworkAgent.RemoteAgents.Add(networkAgent);
				}
			}
			return NetworkAgent.RemoteAgents;
		}

				internal static void DebugSetAllAgentsLocal()
		{
			foreach (NetworkAgent networkAgent in NetworkAgent.AllAgents)
			{
				networkAgent.type = AgentType.Local;
			}
		}

				internal static void OnPostPhysics()
		{
			if (NetworkAgent.AllAgents.Count == 0)
			{
				return;
			}
			foreach (NetworkAgent networkAgent in NetworkAgent.AllAgents)
			{
				networkAgent.CalculateAuthorities();
			}
			NetworkAgent.AllAgents.Sort(AgentRankComparer.Instance);
			for (int i = 0; i < NetworkAgent.AllAgents.Count - 1; i++)
			{
				NetworkAgent networkAgent2 = NetworkAgent.AllAgents[i];
				NetworkAgent networkAgent3 = NetworkAgent.AllAgents[i + 1];
				if (networkAgent3.rank.Level == AgentRankLevel.Slave && networkAgent2.authorities.Contains(networkAgent3.root))
				{
					networkAgent3.rank.SetLevel(AgentRankLevel.None);
					networkAgent3.authorities.Clear();
					NetworkAgent.AllAgents.Sort(AgentRankComparer.Instance);
					i--;
				}
			}
			foreach (NetworkAgent networkAgent4 in NetworkAgent.AllAgents)
			{
				foreach (NetworkAgent networkAgent5 in NetworkAgent.AllAgents)
				{
					if (!(networkAgent4 == networkAgent5))
					{
						networkAgent4.ResolveConflictsAgainst(networkAgent5);
					}
				}
			}
			foreach (NetworkAgent networkAgent6 in NetworkAgent.AllAgents)
			{
				networkAgent6.GenerateAuthorityDeclarations();
			}
		}

				public NetworkAgent()
		{
		}

						static NetworkAgent()
		{
		}

				[CompilerGenerated]
		private void <ResolveConflictsAgainst>g__ReleaseAuth|63_0(NObject n)
		{
			this.authorities.RemoveAllOf(n.TransformGroup);
			if (this.type == AgentType.Local && this.rank.Level > AgentRankLevel.None)
			{
			NetworkAgent networkAgent = NetworkAgent.AllAgents.FirstOrDefault<NetworkAgent>();
			if (this != networkAgent && this.rootAuthChain.Contains(n))
			{
				foreach (NetworkAgent networkAgent2 in NetworkAgent.AllAgents)
				{
					if (!(networkAgent2 == this) && networkAgent2.rank > this.rank && networkAgent2.root.TransformGroup.Contains(n))
					{
						return;
					}
				}
				this.authReleaseRequests.Add(n);
			}
		}
		}

				[Header("Config")]
		[SerializeField]
		private AgentType type;

				[SerializeField]
		private AgentRank rank;

				[SerializeField]
		private Color debugColor;

				[SerializeField]
		private bool releaseMotionlessAuths = true;

				[SerializeField]
		private float rootAuthChainReleaseDelay = 0.25f;

				private const float RemoteAuthChainRecordStaleTimeout = 0.25f;

				private NObject root;

				private NList rootAuthChain;

				private NListTimed recentlyInRootAuthChain;

				private AuthChainTable authChains;

				private NList authorities;

				private NList proxyAuthorities;

				private NList authReleaseRequests;

				private NList conflictsToProcess;

				private AuthDecList authDecs;

				private List<NetworkAgent.AuthChainRecord> authChainRecords;

				private Stack<NetworkAgent.AuthChainRecord> authChainRecordPool;

				private List<NetworkAgent.AuthChainRecord> authChainRecords_SortedByTickNumber;

				private HashSet<ushort> seenNObjectIds;

				private List<byte> declaredChainsForNewestTickNumber;

				private HashSet<byte> receivedChainsForNewestTickNumber;

				private TickNumber newestSeenTickNumber;

				private TickNumber newestCompleteTickNumber;

				private bool hasCompleteRemoteTick;

				private bool authDecsDirty;

				private bool pendingSnap;

				[CompilerGenerated]
		private Action OnDestroyed;

				private static readonly List<NetworkAgent> AllAgents = new List<NetworkAgent>();

				private static readonly List<NetworkAgent> LocalAgents = new List<NetworkAgent>();

				private static readonly List<NetworkAgent> RemoteAgents = new List<NetworkAgent>();

				private class AuthChainRecord
		{
						public AuthChainRecord()
			{
			}

						public uint RemoteTickNumber;

						public byte AuthChainIndex;

						public readonly AuthDecList AuthDecs = new AuthDecList(16);

						public float Age;
		}

				private class NewestRemoteTickNumberFirst : IComparer<NetworkAgent.AuthChainRecord>
		{
						public int Compare(NetworkAgent.AuthChainRecord a, NetworkAgent.AuthChainRecord b)
			{
				return b.RemoteTickNumber.CompareTo(a.RemoteTickNumber);
			}

						public NewestRemoteTickNumberFirst()
			{
			}

									static NewestRemoteTickNumberFirst()
			{
			}

						public static readonly NetworkAgent.NewestRemoteTickNumberFirst Instance = new NetworkAgent.NewestRemoteTickNumberFirst();
		}
	}
}
