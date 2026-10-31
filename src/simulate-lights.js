import 'dotenv/config';

const port = Number(process.env.PORT || 3177);
try {
  const response = await fetch(`http://127.0.0.1:${port}/simulate/lights`, {
    method: 'POST',
    headers: { 'Content-Type': 'text/plain' },
    body: '!lights'
  });
  const result = await response.json();
  console.log(`Simulated !lights: ${result.triggered ? 'Unity scare triggered' : result.reason}`);
  if (!response.ok || !result.triggered) process.exitCode = 1;
} catch (error) {
  console.error(`Simulated !lights failed: ${error.message}`);
  process.exitCode = 1;
}
