<script lang="ts">
    import '$lib/css/mainstyles.css'
    import { onDestroy, onMount } from 'svelte';
    import { userStatuses } from '$lib/stores/userStatusesStore';
    import { fetchUserId, isLoggedIn, type User } from '$lib/Handlers/accountHandler';
    import { browser } from '$app/environment';
    import type { HubConnection } from '@microsoft/signalr';
    import { startConnection } from '$lib/hubs/UserStatusHub';
    import { connectionStore } from '$lib/stores/connectionsStore';
    import {userStore, updateUserInfo} from '$lib/stores/userInfoStore';
    import { fetchFriendRequests, handleAcceptRequest, type FriendRequest, handleRejectRequest } from '$lib/Handlers/userHandler';
    import { slide } from 'svelte/transition';
    import { receivedRequestsStore } from '$lib/stores/friendRequestsStore';
    import { goto } from '$app/navigation';
    import {sidebarOpen} from '$lib/stores/sidebarToggleStore';
    
    const backendUrl = import.meta.env.VITE_BACKEND_URL;

    let connection: HubConnection;
    let userId: string;
    let isLogged: boolean;
    
    let imageUrl = '/images/profile-pictures/placeholder.png'

    let ready: boolean = false;

    $: {
        (async () => {
            isLogged = await isLoggedIn();
        })();
    }

    onMount(async () => {
        connection = await startConnection();
        connectionStore.set(connection);
        console.log("isLogged:" + isLogged);

       
        
        if(isLogged){
            await updateUserInfo();
            userId = await fetchUserId();

            await fetchFriendRequests();
        

            const response = await fetch(`${backendUrl}/account/getActiveUsers`);
            const data = await response.json();
            userStatuses.set(data.activeUsers);
            await connection.invoke("UpdateUserStatus", userId.toString(), "online");
            
            // window.onunload = async () => {
            //     await connection.invoke("UpdateUserStatus", userId.toString(), "offline");
            // };
            
        }
        ready= true;
    });

    onDestroy(() => {
    if (connection) {
        const data = new FormData();
        data.append("userId", userId);
        data.append("status", "offline");

        navigator.sendBeacon(`${backendUrl}/account/updateUserStatus/${userId}/${'offline'}`, data);

        connection.stop();
    }
});
    
    async function logout() {
        console.log("Test function called");
        if (browser){
            connection.invoke("UpdateUserStatus", userId.toString(), "offline");
            document.cookie = 'token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
            
            location.href = '/';
            location.reload();
        }
    }
    let dropdownOpen = false;
    let notificationsOpen = false;

    function toggleDropdown() {
        dropdownOpen = !dropdownOpen;
        notificationsOpen = false;
    }
    function toggleNotifications() {
        notificationsOpen = !notificationsOpen;
        dropdownOpen = false;
    }

    async function acceptRequest(request: FriendRequest){
        await handleAcceptRequest(request.username);
    }
    
    async function rejectRequest(request: FriendRequest){
    await handleRejectRequest(request.username);
    $receivedRequestsStore = $receivedRequestsStore.filter(req => req.username !== request.username);
}

function toggleSidebar() {
        sidebarOpen.update(value => !value);
        console.log(sidebarOpen);
    }
</script>
    
<svelte:window on:click={() => {notificationsOpen = false; dropdownOpen=false;} } />

<div class="container-fluid">
    <nav class="navbar navbar-expand-lg navbar-light">
        {#if isLogged}
        <button class="sidebar-toggle" on:click={toggleSidebar}>
            <i class="fa-solid fa-bars"></i>
        </button>
        {/if}
        <a class="navbar-brand" on:click={() => isLogged ? goto('/chat/home') : goto('/welcome')}>Boomerangr</a>
        <div class="dropdown-wrap">
            {#if isLogged}
            
            <!-- svelte-ignore a11y-click-events-have-key-events -->
            <div class="notifications" on:click|stopPropagation={toggleNotifications} role="button" tabindex=0>
                
                <i class="fa fa-envelope fa-lg" aria-hidden="true" style="color: white;"></i>
                
                {#if notificationsOpen}
                <div class="dropdown-menu" class:show={notificationsOpen} transition:slide={{duration: 500}}>
                    {#if $receivedRequestsStore.length === 0}
                        <span><i>No friend requests</i></span>
                    {:else}
                    {#each $receivedRequestsStore as request}
                        <div class="friend-request-wrap">
                            <div class="display-flex flex-column" style="line-height:2rem; max-width:12rem">
                                <span style="width: 100%;">{request.username} sent you a friend request</span>
                                <span style="font-size:1rem;"><i>{request.requestSentDate}</i></span>
                            </div>
                            <div class="mb-3">
                                <button class="btn btn-success btn-sm w-100 mb-1" on:click|stopPropagation={() => acceptRequest(request)}>Accept</button>
                                <button class="btn btn-danger btn-sm w-100" on:click|stopPropagation={() => rejectRequest(request)}>Decline</button>
                            </div>
                        </div>  
                        
                    {/each}
                    {/if}   
                </div>
                {/if}
            
            </div>
            <ul class="navbar-nav align-items-center">
                <li class="nav-item dropdown">
                    <a class="nav-link dropdown-toggle" href="#" id="navbarDropdownMenuLink" data-toggle="dropdown" aria-haspopup="true" aria-expanded={dropdownOpen} on:click|stopPropagation={toggleDropdown}>
                    <img alt=" " src={$userStore?.profilePictureUrl} width="35px" height="35px" style="background-color:gray; border-radius:50%">
                    <span class="username">{$userStore?.userName}</span>                                    
                    </a>    
                    {#if dropdownOpen}
                        <div class="dropdown-menu" style="padding:0 !important;" class:show={dropdownOpen} transition:slide={{duration:500}} >
                        <a class="dropdown-item" on:click={() => goto("/account")}>Account</a>
                        <a class="dropdown-item" href="#">Another action</a>
                        <div class="dropdown-divider mb-0"></div>
                        <a class="dropdown-item logout-item" href="#" on:click={logout}>Logout</a>
                        </div>
                    {/if}
                </li>
            </ul>
            {/if}
        </div>
    </nav>  
    <main>
        <slot/>
    </main>
</div>


<style>
    .notifications{
        border-radius: 25%;
        padding: 0.25rem 0.5rem 0.25rem 0.5rem;
        position: relative;
    }
    .notifications:hover{
        background-color: #ffffffa9;
    }
    .friend-request-wrap{
        width:100%;
        display: flex;
        flex-direction: row;
        
    }
    .dropdown-wrap{
        display: flex;
        flex-direction: row;
        margin-left: auto;
        margin-right: 4rem;
        align-items: center;
    }
    .dropdown-menu{
        min-width: 20rem;
        border-bottom-left-radius: 10px;
        border-bottom-right-radius: 10px;
        position: absolute;
        top: 115%;
        left: -7rem;
        max-height: 8rem;
        padding: 0;
    }
        .show{
            padding: 0;
        }
    .dropdown-toggle{
        margin-left: 0.5rem;
    }
    .dropdown-item {
    padding: 0.25rem 0.25rem; /* Adjust padding as needed */
    white-space: normal; /* Allow text to wrap to next line */
    text-align: left; /* Align text to the left */
    left:0;
    color: #777777 !important;
    margin-left:0.40rem;
   
    
    
    }
        .logout-item{
            background-color: #DA373C;
            margin:0;
            color:white !important;
            border-bottom-left-radius: 5px;
            border-bottom-right-radius: 5px;
        }
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
                margin-left: 1rem;
            }

    
    .sidebar-toggle{
        border:none;
        background:transparent;
    }
        .sidebar-toggle i{
            color: var(--prim-fg);
            font-size: 1.5rem;
        }

        
        @media only screen and (max-width: 600px) {
        .username{
            display: none;
        }
        .dropdown-wrap{
            margin-right: 1rem;
        }
        .dropdown-menu {
            top: 100%;
            left: -6rem;
             /* make the dropdown full width */
            width: 10rem;
            min-width: 0;
        }
    }
</style>






