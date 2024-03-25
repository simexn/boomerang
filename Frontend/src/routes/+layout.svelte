<script lang="ts">
    import '$lib/css/mainstyles.css'
    import { onMount } from 'svelte';
    import {fetchChats} from '$lib/Handlers/groupHandler'
    import {getToken} from '$lib/Handlers/authHandler'
    import { userStatuses } from '$lib/stores/userStatusesStore';
    import { fetchUserId, isLoggedIn } from '$lib/Handlers/accountHandler';
    import { browser } from '$app/environment';
    import type { HubConnection } from '@microsoft/signalr';
    import { startConnection } from '$lib/hubs/UserStatusHub';
    
    const backendUrl = import.meta.env.VITE_BACKEND_URL;

    let connection: HubConnection;
    let userId: string;

    onMount(async () => {
        connection = await startConnection();
        
        if(await isLoggedIn()){
            userId = await fetchUserId();
            connection.invoke("UpdateUserStatus", userId, "online");
        }
    });
    
    async function logout() {
        console.log("Test function called");
        if (browser){
            userId = await fetchUserId();
            await connection.invoke("UpdateUserStatus", userId, "offline");
            document.cookie = 'token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
            
            location.href = '/';
            location.reload();
        }
    }
</script>
    

<div class="container-fluid">
    <nav class="navbar navbar-expand-lg navbar-light">
        <a class="navbar-brand" href="/">Boomerangr</a>
        <button on:click = {async () => await logout()}></button>
    </nav>  
    <main>
        <slot/>
    </main>
</div>


<style>
    .container-fluid {
        box-sizing: border-box;
        padding:0;
        display: flex;
        flex-direction: column;
        height: 100vh;
        overflow: hidden;
    }
        main {
            height:100%;
            display: flex;
            flex-direction: row;
            overflow: hidden;
        }
            nav{
                background-color: var(--prim);
            }
            a{
                color: var(--prim-fg) !important;
                margin-left: 2rem;
            }
</style>






