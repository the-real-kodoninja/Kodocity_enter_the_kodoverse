import Principal "mo:base/Principal";
import HashMap "mo:base/HashMap";
import Text "mo:base/Text";
import Nat "mo:base/Nat";
import Float "mo:base/Float";
import Iter "mo:base/Iter";
import Array "mo:base/Array";

actor Kodoverse {
  type PlayerId = Principal;
  type LandId = Nat;
  type AgentId = Text;
  type Position = {
    x: Float;
    y: Float;
    z: Float;
  };

  type Land = {
    id: LandId;
    owner: PlayerId;
    position: Position;
    size: Float; // Size in square meters
    price: Float; // Price in ICP
    structure: ?Text; // Name of the structure built (e.g., "House", "Store")
  };

type Player = {
    id: PlayerId;
    position: Position;
    ownedLands: [LandId];
    agentId: ?AgentId;
    health: Float;
    hasInsurance: Bool;
    cryptoBalance: Float;
    walletBalance: Float; // Balance in the wallet (accessible but vulnerable)
    vaultBalance: Float;  // Balance in the vault (secure but less accessible)
};

  type Agent = {
    id: AgentId;
    owner: PlayerId;
    position: Position;
    needs: {
      hunger: Float;
      sleep: Float;
      // Sex: Float;
    };
    state: Text; // "Normal" or "Rogue"
};

  private var players = HashMap.HashMap<PlayerId, Player>(0, Principal.equal, Principal.hash);
  private var lands = HashMap.HashMap<LandId, Land>(0, Nat.equal, func(n) { n });
  private var agents = HashMap.HashMap<AgentId, Agent>(0, Text.equal, Text.hash);
  private var nextLandId: Nat = 0;

  // Register a player with health and insurance
public shared(msg) func registerPlayer(agentId: ?AgentId): async Bool {
    let playerId = msg.caller;
    switch (players.get(playerId)) {
        case (?_) { false };
        case (null) {
            let newPlayer: Player = {
                id = playerId;
                position = { x = 0.0; y = 1.0; z = 0.0 };
                ownedLands = [];
                agentId = agentId;
                health = 100.0;
                hasInsurance = false;
                cryptoBalance = 10.0; // Starting balance
            };
            players.put(playerId, newPlayer);
            true
        };
    }
};

// Update player health, balance, wallet, and vault
public shared(msg) func updatePlayerHealthAndBalance(health: Float, walletBalance: Float, vaultBalance: Float): async Bool {
    let playerId = msg.caller;
    switch (players.get(playerId)) {
        case (null) { false };
        case (?player) {
            let updatedPlayer: Player = {
                id = player.id;
                position = player.position;
                ownedLands = player.ownedLands;
                agentId = player.agentId;
                health = health;
                hasInsurance = player.hasInsurance;
                cryptoBalance = player.cryptoBalance; // Deprecated, kept for compatibility
                walletBalance = walletBalance;
                vaultBalance = vaultBalance;
            };
            players.put(playerId, updatedPlayer);
            true
        };
    }
};

// Transfer balance to vault
public shared(msg) func transferToVault(amount: Float): async Bool {
    let playerId = msg.caller;
    switch (players.get(playerId)) {
        case (null) { false };
        case (?player) {
            if (player.walletBalance < amount) { return false };
            let updatedPlayer: Player = {
                id = player.id;
                position = player.position;
                ownedLands = player.ownedLands;
                agentId = player.agentId;
                health = player.health;
                hasInsurance = player.hasInsurance;
                cryptoBalance = player.cryptoBalance;
                walletBalance = player.walletBalance - amount;
                vaultBalance = player.vaultBalance + amount;
            };
            players.put(playerId, updatedPlayer);
            true
        };
    }
};

// Withdraw from vault (simplified, in reality would have a delay or security check)
public shared(msg) func withdrawFromVault(amount: Float): async Bool {
    let playerId = msg.caller;
    switch (players.get(playerId)) {
        case (null) { false };
        case (?player) {
            if (player.vaultBalance < amount) { return false };
            let updatedPlayer: Player = {
                id = player.id;
                position = player.position;
                ownedLands = player.ownedLands;
                agentId = player.agentId;
                health = player.health;
                hasInsurance = player.hasInsurance;
                cryptoBalance = player.cryptoBalance;
                walletBalance = player.walletBalance + amount;
                vaultBalance = player.vaultBalance - amount;
            };
            players.put(playerId, updatedPlayer);
            true
        };
    }
};

// Purchase insurance
public shared(msg) func purchaseInsurance(): async Bool {
    let playerId = msg.caller;
    switch (players.get(playerId)) {
        case (null) { false };
        case (?player) {
            if (player.cryptoBalance < 2.0) { return false }; // Insurance costs 2 ICP
            let updatedPlayer: Player = {
                id = player.id;
                position = player.position;
                ownedLands = player.ownedLands;
                agentId = player.agentId;
                health = player.health;
                hasInsurance = true;
                cryptoBalance = player.cryptoBalance - 2.0;
            };
            players.put(playerId, updatedPlayer);
            true
        };
    }
};

  // Update player position
  public shared(msg) func updatePosition(x: Float, y: Float, z: Float): async Bool {
    let playerId = msg.caller;
    switch (players.get(playerId)) {
      case (null) { false };
      case (?player) {
        let updatedPlayer: Player = {
          id = player.id;
          position = { x; y; z };
          ownedLands = player.ownedLands;
          agentId = player.agentId;
        };
        players.put(playerId, updatedPlayer);
        true
      };
    }
  };

  // Update agent position and needs
public shared(msg) func updateAgent(agentId: AgentId, x: Float, y: Float, z: Float, hunger: Float, sleep: Float, state: Text): async Bool {
    switch (agents.get(agentId)) {
        case (null) { false };
        case (?agent) {
            if (agent.owner != msg.caller) { return false };
            let updatedAgent: Agent = {
                id = agent.id;
                owner = agent.owner;
                position = { x; y; z };
                needs = { hunger; sleep };
                state = state;
            };
            agents.put(agentId, updatedAgent);
            true
        };
    }
};

// Mark an agent as rogue
public shared(msg) func markAgentRogue(agentId: AgentId): async Bool {
    switch (agents.get(agentId)) {
        case (null) { false };
        case (?agent) {
            if (agent.owner != msg.caller) { return false };
            let updatedAgent: Agent = {
                id = agent.id;
                owner = agent.owner;
                position = agent.position;
                needs = agent.needs;
                state = "Rogue";
            };
            agents.put(agentId, updatedAgent);
            true
        };
    }
};

  // Purchase land
  public shared(msg) func purchaseLand(position: Position, size: Float, price: Float): async ?LandId {
    let landId = nextLandId;
    nextLandId += 1;

    let land: Land = {
      id = landId;
      owner = msg.caller;
      position = position;
      size = size;
      price = price;
      structure = null;
    };

    lands.put(landId, land);

    switch (players.get(msg.caller)) {
      case (null) { return null };
      case (?player) {
        let updatedPlayer: Player = {
          id = player.id;
          position = player.position;
          ownedLands = Array.append(player.ownedLands, [landId]);
          agentId = player.agentId;
        };
        players.put(msg.caller, updatedPlayer);
      };
    };

    ?landId
  };

  // Build a structure on land
  public shared(msg) func buildStructure(landId: LandId, structure: Text): async Bool {
    switch (lands.get(landId)) {
      case (null) { false };
      case (?land) {
        if (land.owner != msg.caller) { return false };
        let updatedLand: Land = {
          id = land.id;
          owner = land.owner;
          position = land.position;
          size = land.size;
          price = land.price;
          structure = ?structure;
        };
        lands.put(landId, updatedLand);
        true
      };
    }
  };

  // Get player data
  public query func getPlayer(playerId: PlayerId): async ?Player {
    players.get(playerId)
  };

  // Get agent data
  public query func getAgent(agentId: AgentId): async ?Agent {
    agents.get(agentId)
  };

  // Get land data
  public query func getLand(landId: LandId): async ?Land {
    lands.get(landId)
  };

  // Get all lands
  public query func getAllLands(): async [(LandId, Land)] {
    Iter.toArray(lands.entries())
  };
};
