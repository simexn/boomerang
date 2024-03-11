<svelte:head>
    <title>Chat App - Home</title>
</svelte:head>

<div class="home-container">
    <h1>Welcome to Our Chat App!</h1>
    <p>This is a place where you can connect with your friends, family, and colleagues in real-time.</p>
    <p>To get started, click on a chat room on the left, or create a new one by clicking the '+' button.</p>
    <p>Enjoy chatting!</p>
</div>

<style>
  .home-container {
      flex-grow: 1;
      padding: 2rem;
      background-color: var(--main);
      color: var(--text);
  }
  
  .home-container h1 {
      font-size: 2rem;
      margin-bottom: 1rem;
  }
  
  .home-container p {
      font-size: 1.2rem;
      line-height: 1.5;
      margin-bottom: 1rem;
  }
  </style>