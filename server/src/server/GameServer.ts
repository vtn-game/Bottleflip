import express, { Express } from 'express';
import http from 'http';
import path from 'path';
import { WebSocketServer } from 'ws';
import { ServerConfig } from '../config/Config';
import { ConnectionManager } from './ConnectionManager';
import { MessageRouter } from './MessageRouter';

export class GameServer {
    private config: ServerConfig;
    private app: Express;
    private httpServer: http.Server;
    private wss: WebSocketServer;
    private connectionManager: ConnectionManager;
    private messageRouter: MessageRouter;

    constructor(config: ServerConfig) {
        this.config = config;
        this.app = express();
        this.httpServer = http.createServer(this.app);
        this.wss = new WebSocketServer({ port: config.wsPort });
        this.connectionManager = new ConnectionManager();
        this.messageRouter = new MessageRouter(this.connectionManager);

        this.setupExpress();
        this.setupWebSocket();
    }

    private setupExpress(): void {
        // CORS設定
        this.app.use((req, res, next) => {
            res.header('Access-Control-Allow-Origin', this.config.corsOrigin);
            res.header('Access-Control-Allow-Headers', 'Origin, X-Requested-With, Content-Type, Accept');
            next();
        });

        // JSON パース
        this.app.use(express.json());

        // 静的ファイル配信（WebGLビルド）
        const webRoot = path.resolve(__dirname, '../../', this.config.webRoot);
        this.app.use(express.static(webRoot));

        // ヘルスチェック
        this.app.get('/api/health', (req, res) => {
            res.json({
                status: 'ok',
                connections: this.connectionManager.getConnectionCount(),
                uptime: process.uptime()
            });
        });

        // ゲーム状態取得
        this.app.get('/api/state', (req, res) => {
            res.json({
                success: true,
                data: this.connectionManager.getGameState()
            });
        });
    }

    private setupWebSocket(): void {
        this.wss.on('connection', (ws, req) => {
            const clientIp = req.socket.remoteAddress || 'unknown';
            console.log(`[WS] New connection from ${clientIp}`);

            const connection = this.connectionManager.addConnection(ws);

            ws.on('message', (data) => {
                try {
                    const message = JSON.parse(data.toString());
                    this.messageRouter.route(connection, message);
                } catch (err) {
                    console.error('[WS] Invalid message:', err);
                }
            });

            ws.on('close', () => {
                console.log(`[WS] Connection closed: ${connection.id}`);
                this.connectionManager.removeConnection(connection.id);
            });

            ws.on('error', (err) => {
                console.error(`[WS] Error on ${connection.id}:`, err);
            });
        });
    }

    async start(): Promise<void> {
        return new Promise((resolve) => {
            this.httpServer.listen(this.config.httpPort, () => {
                console.log(`[HTTP] Server listening on port ${this.config.httpPort}`);
                console.log(`[WS] WebSocket server listening on port ${this.config.wsPort}`);
                console.log('');
                console.log('Server is ready!');
                resolve();
            });
        });
    }

    async stop(): Promise<void> {
        this.wss.close();
        this.httpServer.close();
        console.log('Server stopped.');
    }
}
