<script lang="ts">
    import type { User } from "$lib/handlers/accountHandler";
    import type { Group } from "$lib/handlers/groupHandler";
    import { fly } from "svelte/transition";
    import { handleKickUser, handlePromoteUser, handleDemoteUser, handleTransferOwnership, handleBanUser } from "$lib/handlers/groupHandler";
    import "$lib/css/sidebarstyles.css"

    export let groupInfo: Group;
    export let userSidebarDropdown: any;
    export let isUsersSidebarOpen: boolean;
    export let userInfo: User;
    export let imageUrl: string;
    export let chatId: string;

    let isUserHovered = false;

    export async function kickUser(userId: number){
        await handleKickUser(chatId, userId);
        //groupInfo.users = groupInfo.users.filter(user => user.id !== userId);
    }
    export async function banUser(userId: number){
        await handleBanUser(chatId, userId);
    }

    export async function promoteUser(userId: number){
        await handlePromoteUser(chatId, userId);
    }
    export async function demoteUser(userId: number){
        await handleDemoteUser(chatId, userId);
    }
    export async function transferOwnership(userId: number){
        await handleTransferOwnership(chatId, userId);
    }
    function openDropdown(userId : any, event: any) {
        event.stopPropagation();
        userSidebarDropdown = userSidebarDropdown === userId ? null : userId;
    }
    function closeDropdown() {
    userSidebarDropdown = null;
  }
</script>
<svelte:window on:click={closeDropdown} />
<div class="users-sidebar d-flex flex-column" transition:fly="{{x: 1000, duration: 500}}">
    <div class="sidebar-header">
        <span class="sidebar-title">
            <span style="line-height: 2.4rem;font-family: Metropolis, sans-serif !important;">
                Членове
            </span>
            <span class="sidebar-subtitle">{groupInfo.users.length} члена</span>
        </span><button class="sidebar-close-btn" aria-label="Close" on:click={() => isUsersSidebarOpen = false}>
            <i class="fa-solid fa-x"></i>
        </button>
    </div>
    <h5>Администратори: </h5>
    {#each groupInfo.users as user (user.id)}
        {#if groupInfo.admins.some(admin => admin.id === user.id)}
            <div class="user-section">
                <div role="navigation" class="user d-flex d-row align-items-center" 
                    on:focus="{() => isUserHovered = true}" 
                    on:mouseover="{() => isUserHovered = true}" 
                    on:blur="{() => isUserHovered = false}" 
                    on:mouseout="{() => isUserHovered = false}">
                    <div class="d-flex d-row align-items-center">
                        <img width="20px" height="20px" src={imageUrl} alt={user.userName} />
                        <p class="pb-0 mb-0">{user.userName}</p>
                    </div>
                    <div class="d-flex flex-row">
                        <i role="navigation" class="dots fa-solid fa-ellipsis-vertical" on:click|stopPropagation={(event) => openDropdown(user.id, event)}></i>
                        {#if user.id === userSidebarDropdown}
                            <div role="navigation" class="dropdown-menu show">
                                {#if user.id !== userInfo.id && (groupInfo.creatorId === userInfo.id || (groupInfo.admins.some(admin => admin.id === userInfo.id) && user.id !== groupInfo.creatorId))}
                                    {#if groupInfo.creatorId === userInfo.id}
                                        <button class="dropdown-item" on:click|stopPropagation={() => transferOwnership(user.id)}>Смяна на собственик</button>
                                        {#if groupInfo.admins.some(admin => admin.id === user.id)}
                                            <button class="dropdown-item" on:click|stopPropagation={() => demoteUser(user.id)}>Принижи към член</button>
                                        {/if}
                                    {/if}
                                    <div class="dropdown-divider"></div>
                                    <button class="dropdown-item" on:click|stopPropagation={() => kickUser(user.id)}>Изхвърляне</button>
                                    <button class="dropdown-item" on:click|stopPropagation={() => banUser(user.id)}>Забрани достъп</button>
                                {/if}
                            </div>
                        {/if}
                    </div>
                </div>
            </div>
        {/if}
    {/each}
    <h5>Членове: </h5>
    {#each groupInfo.users as user (user.id)}
        {#if !groupInfo.admins.some(admin => admin.id === user.id)}
            <div class="user-section">
                <div role="navigation" class="user d-flex d-row align-items-center" 
                    on:focus="{() => isUserHovered = true}" 
                    on:mouseover="{() => isUserHovered = true}" 
                    on:blur="{() => isUserHovered = false}" 
                    on:mouseout="{() => isUserHovered = false}">
                    <div class="d-flex d-row align-items-center">
                        <img width="20px" height="20px" src={imageUrl} alt={user.userName} />
                        <p class="pb-0 mb-0">{user.userName}</p>
                    </div>
                    <div class="d-flex flex-row">
                        <i class="dots fa-solid fa-ellipsis-vertical" on:click|stopPropagation={(event) => openDropdown(user.id, event)}></i>
                        {#if user.id === userSidebarDropdown}
                            <div role="navigation" class="dropdown-menu show">
                                {#if user.id !== userInfo.id && (groupInfo.creatorId === userInfo.id || (groupInfo.admins.some(admin => admin.id === userInfo.id) && user.id !== groupInfo.creatorId))}
                                    {#if groupInfo.creatorId === userInfo.id}
                                        <button class="dropdown-item" on:click|stopPropagation={() => transferOwnership(user.id)}>Смяна на собственик</button>
                                        {#if groupInfo.admins.some(admin => admin.id === user.id)}
                                            <button class="dropdown-item" on:click|stopPropagation={null}>Понижи към член</button>
                                        {/if}
                                    {/if}
                                    {#if groupInfo.creatorId === userInfo.id || groupInfo.admins.some(admin => admin.id === userInfo.id)}
                                        <button class="dropdown-item" on:click|stopPropagation={() => promoteUser(user.id)}>Повиши към админ</button>
                                    {/if}
                                    <div class="dropdown-divider"></div>
                                    <button class="dropdown-item" on:click|stopPropagation={() => kickUser(user.id)}>Изхвърляне</button>
                                    <button class="dropdown-item" on:click|stopPropagation={() => banUser(user.id)}>Забрани достъп</button>
                                {/if}
                            </div>
                        {/if}
                    </div>
                </div>
            </div>
        {/if}
    {/each}
</div>

<style>
    .user-section{
        margin-bottom: 10px;
    }
        .user{
            position:relative;
            display:flex;
            justify-content: space-between;
            align-items: center;
        }
            .dropdown-menu{
                position: absolute;
                top: 100%; /* This positions the dropdown right under the user div */
                right: 0;
            }
            .dots{
                visibility: hidden;
                margin-left: 15px;
                margin-right: 15px;
                padding-left:10px;
                padding-right:10px;
                padding-top:2px;
                padding-bottom:2px;
                border-radius: 10px;
                
            }
                
                
        .user:hover .dots{
            visibility: visible;
            
            
        }
            .user:hover .dots:hover{
                cursor: pointer;
                background-color: #a8a8a8;
            }
        .user:hover{
            background-color: #e5e5e5;
        }
        .user img{
            margin-right:5px
        }
</style>