import { GameServer } from './server/GameServer';
import { Config } from './config/Config';

async function main() {
    console.log('=================================');
    console.log(' Bottle Flip Server Starting...');
    console.log('=================================');

    const config = Config.load();
    const server = new GameServer(config);

    // シャットダウンハンドリング
    process.on('SIGINT', async () => {
        console.log('\nShutting down...');
        await server.stop();
        process.exit(0);
    });

    process.on('SIGTERM', async () => {
        console.log('\nShutting down...');
        await server.stop();
        process.exit(0);
    });

    await server.start();
}

main().catch((err) => {
    console.error('Failed to start server:', err);
    process.exit(1);
});
