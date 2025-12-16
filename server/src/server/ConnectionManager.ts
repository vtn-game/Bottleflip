import { WebSocket } from 'ws';
import { v4 as uuidv4 } from 'uuid';

export enum ClientType {
    Unknown = 'unknown',
    Web = 'web',      // スマホWebアプリ
    Main = 'main'     // 母艦アプリ
}

export interface Connection {
    id: string;
    ws: WebSocket;
    clientType: ClientType;
    playerId?: string;
    playerName?: string;
    connectedAt: Date;
    lastActivityAt: Date;
}

export interface GameState {
    webClients: number;
    mainClients: number;
    players: { id: string; name: string }[];
}

export class ConnectionManager {
    private connections: Map<string, Connection> = new Map();

    addConnection(ws: WebSocket): Connection {
        const connection: Connection = {
            id: uuidv4(),
            ws,
            clientType: ClientType.Unknown,
            connectedAt: new Date(),
            lastActivityAt: new Date()
        };
        this.connections.set(connection.id, connection);
        return connection;
    }

    removeConnection(connectionId: string): void {
        const connection = this.connections.get(connectionId);
        if (connection) {
            this.connections.delete(connectionId);
            // 他のクライアントに通知
            if (connection.playerId) {
                this.broadcastToMain({
                    type: 'player_left',
                    data: {
                        playerId: connection.playerId,
                        playerName: connection.playerName
                    }
                });
            }
        }
    }

    getConnection(connectionId: string): Connection | undefined {
        return this.connections.get(connectionId);
    }

    updateConnection(connectionId: string, updates: Partial<Connection>): void {
        const connection = this.connections.get(connectionId);
        if (connection) {
            Object.assign(connection, updates, { lastActivityAt: new Date() });
        }
    }

    getConnectionCount(): number {
        return this.connections.size;
    }

    getGameState(): GameState {
        const webClients = Array.from(this.connections.values())
            .filter(c => c.clientType === ClientType.Web);
        const mainClients = Array.from(this.connections.values())
            .filter(c => c.clientType === ClientType.Main);

        const players = webClients
            .filter(c => c.playerId && c.playerName)
            .map(c => ({ id: c.playerId!, name: c.playerName! }));

        return {
            webClients: webClients.length,
            mainClients: mainClients.length,
            players
        };
    }

    // 特定の接続に送信
    send(connectionId: string, message: object): void {
        const connection = this.connections.get(connectionId);
        if (connection && connection.ws.readyState === WebSocket.OPEN) {
            connection.ws.send(JSON.stringify(message));
        }
    }

    // 全員にブロードキャスト
    broadcast(message: object): void {
        const data = JSON.stringify(message);
        this.connections.forEach((connection) => {
            if (connection.ws.readyState === WebSocket.OPEN) {
                connection.ws.send(data);
            }
        });
    }

    // 母艦アプリにのみブロードキャスト
    broadcastToMain(message: object): void {
        const data = JSON.stringify(message);
        this.connections.forEach((connection) => {
            if (connection.clientType === ClientType.Main &&
                connection.ws.readyState === WebSocket.OPEN) {
                connection.ws.send(data);
            }
        });
    }

    // Webクライアントにのみブロードキャスト
    broadcastToWeb(message: object): void {
        const data = JSON.stringify(message);
        this.connections.forEach((connection) => {
            if (connection.clientType === ClientType.Web &&
                connection.ws.readyState === WebSocket.OPEN) {
                connection.ws.send(data);
            }
        });
    }

    // 特定のプレイヤーに送信
    sendToPlayer(playerId: string, message: object): void {
        const connection = Array.from(this.connections.values())
            .find(c => c.playerId === playerId);
        if (connection && connection.ws.readyState === WebSocket.OPEN) {
            connection.ws.send(JSON.stringify(message));
        }
    }
}
