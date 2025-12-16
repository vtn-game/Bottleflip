import { ConnectionManager, Connection, ClientType } from './ConnectionManager';

export interface Message {
    type: string;
    data?: any;
    timestamp?: number;
}

type MessageHandler = (connection: Connection, message: Message) => void;

export class MessageRouter {
    private connectionManager: ConnectionManager;
    private handlers: Map<string, MessageHandler> = new Map();

    constructor(connectionManager: ConnectionManager) {
        this.connectionManager = connectionManager;
        this.registerDefaultHandlers();
    }

    private registerDefaultHandlers(): void {
        // クライアント登録
        this.register('register', this.handleRegister.bind(this));

        // 認証（Webクライアント用）
        this.register('auth', this.handleAuth.bind(this));

        // ボトル投げ
        this.register('throw', this.handleThrow.bind(this));

        // 投げ結果（母艦から）
        this.register('throw_result', this.handleThrowResult.bind(this));

        // コメント投稿
        this.register('comment', this.handleComment.bind(this));

        // コメントスキップ
        this.register('skip_comment', this.handleSkipComment.bind(this));

        // Ping/Pong
        this.register('ping', this.handlePing.bind(this));
    }

    register(type: string, handler: MessageHandler): void {
        this.handlers.set(type, handler);
    }

    route(connection: Connection, message: Message): void {
        const handler = this.handlers.get(message.type);
        if (handler) {
            console.log(`[Router] ${message.type} from ${connection.id}`);
            handler(connection, message);
        } else {
            console.warn(`[Router] Unknown message type: ${message.type}`);
        }
    }

    // ========== ハンドラー実装 ==========

    private handleRegister(connection: Connection, message: Message): void {
        const { clientType } = message.data || {};

        if (clientType === 'web') {
            this.connectionManager.updateConnection(connection.id, {
                clientType: ClientType.Web
            });
            this.connectionManager.send(connection.id, {
                type: 'register_success',
                data: { connectionId: connection.id }
            });
            console.log(`[Router] Registered as Web client: ${connection.id}`);

        } else if (clientType === 'main') {
            this.connectionManager.updateConnection(connection.id, {
                clientType: ClientType.Main
            });
            this.connectionManager.send(connection.id, {
                type: 'register_success',
                data: {
                    connectionId: connection.id,
                    state: this.connectionManager.getGameState()
                }
            });
            console.log(`[Router] Registered as Main client: ${connection.id}`);
        }
    }

    private handleAuth(connection: Connection, message: Message): void {
        const { playerId, playerName } = message.data || {};

        this.connectionManager.updateConnection(connection.id, {
            playerId,
            playerName
        });

        // 認証成功を返す
        this.connectionManager.send(connection.id, {
            type: 'auth_success',
            data: { playerId, playerName }
        });

        // 母艦にプレイヤー参加を通知
        this.connectionManager.broadcastToMain({
            type: 'player_joined',
            data: { playerId, playerName }
        });

        console.log(`[Router] Player authenticated: ${playerName} (${playerId})`);
    }

    private handleThrow(connection: Connection, message: Message): void {
        const { bottleId, intensity } = message.data || {};

        // 母艦にボトル投げを転送
        this.connectionManager.broadcastToMain({
            type: 'throw',
            data: {
                playerId: connection.playerId,
                playerName: connection.playerName,
                bottleId,
                intensity,
                timestamp: Date.now()
            }
        });

        console.log(`[Router] Throw from ${connection.playerName}: intensity=${intensity}`);
    }

    private handleThrowResult(connection: Connection, message: Message): void {
        // 母艦からの結果を該当プレイヤーに転送
        const { playerId, success, coinsEarned } = message.data || {};

        this.connectionManager.sendToPlayer(playerId, {
            type: 'throw_result',
            data: { success, coinsEarned }
        });

        // 全体にも通知（アクティビティ用）
        this.connectionManager.broadcast({
            type: 'flip_activity',
            data: {
                playerId,
                playerName: message.data.playerName,
                success
            }
        });

        console.log(`[Router] Throw result for ${playerId}: ${success ? 'SUCCESS' : 'FAIL'}`);
    }

    private handleComment(connection: Connection, message: Message): void {
        const { text } = message.data || {};

        // 母艦にコメントを転送
        this.connectionManager.broadcastToMain({
            type: 'comment',
            data: {
                playerId: connection.playerId,
                playerName: connection.playerName,
                text,
                timestamp: Date.now()
            }
        });

        console.log(`[Router] Comment from ${connection.playerName}: ${text}`);
    }

    private handleSkipComment(connection: Connection, message: Message): void {
        // 母艦にスキップを通知
        this.connectionManager.broadcastToMain({
            type: 'skip_comment',
            data: {
                playerId: connection.playerId
            }
        });

        console.log(`[Router] Comment skipped by ${connection.playerName}`);
    }

    private handlePing(connection: Connection, message: Message): void {
        this.connectionManager.send(connection.id, {
            type: 'pong',
            data: { timestamp: Date.now() }
        });
    }
}
