export interface ServerConfig {
    httpPort: number;
    wsPort: number;
    webRoot: string;
    corsOrigin: string;
}

export class Config {
    static load(): ServerConfig {
        return {
            httpPort: parseInt(process.env.HTTP_PORT || '8080'),
            wsPort: parseInt(process.env.WS_PORT || '8081'),
            webRoot: process.env.WEB_ROOT || '../unityWeb/Build',
            corsOrigin: process.env.CORS_ORIGIN || '*'
        };
    }
}
